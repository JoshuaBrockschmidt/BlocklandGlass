function GlassLiveRoom::create(%id, %name) {
  if(isObject(GlassLive.room[%id]))
    return GlassLive.room[%id];

  %room = new ScriptObject() {
    class = "GlassLiveRoom";

    id = %id;
    name = %name;

    users = "";
    window = "";
  };

  GlassLive.room[%id] = %room;



  return %room;
}

function GlassLiveRoom::getFromId(%id) {
  if(isObject(GlassLive.room[%id]))
    return GlassLive.room[%id];
  else
    return false;
}

function GlassLiveRoom::leave(%this, %conf) {
  if(!%conf) {
    %this = GlassLiveRoom::getFromId(%this);
    messageBoxYesNo("Are you sure?", "Are you sure you want to leave this room?", "GlassLiveRoom::leaveRoom(" @ %this.getId() @ ", true);");
  } else {
    echo(%this);
    %this.window.deleteAll();
    %this.window.delete();

    %obj = JettisonObject();
    %obj.set("type", "string", "roomLeave");
    %obj.set("id", "string", %this.id);

    GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
  }
}

function GlassLiveRoom::addUser(%this, %blid) {
  if(%this.user[%blid])
    return;

  %this.user[%blid] = true;
  %this.users = trim(%this.users TAB %blid);

  if(isObject(%this.window)) {
    %this.renderUserList();
  }
}

function GlassLiveRoom::removeUser(%this, %blid) {
  if(!%this.user[%blid])
    return;

  %this.user[%blid] = false;
  for(%i = 0; %i < getFieldCount(%this.users); %i++) {
    if(getField(%this.users, %i) == %blid) {
      %this.users = removeField(%this.users, %i);
      return;
    }
  }
}

function GlassLiveRoom::getCount(%this) {
  return getFieldCount(%this.users);
}

function GlassLiveRoom::getBl_id(%this, %idx) {
  return getField(%this.users, %idx);
}

function GlassLiveRoom::getUser(%this, %idx) {
  return GlassLiveUser::getFromBlid(%this.getBl_id(%idx));
}

function GlassLiveRoom::createWindow(%this) {
  if(isObject(%this.window)) {
    %this.window.deleteAll();
    %this.window.delete();
  }

  %gui = GlassLive::createChatroomGui(%this.id);
  %gui.title = %this.title;
  %gui.id = %this.id;
  %gui.position = "100 100";

  %this.window = %gui;

  %this.renderUserList();

  GlassOverlayGui.add(%gui);
  return %gui;
}

function GlassLiveRoom::onUserJoin(%this, %blid) {
  %user = GlassLiveUser::getFromBlid(%blid);
  %text = "<font:verdana:12><color:666666>" @ %user.username @ " entered the room.";
  %this.pushText(%text);
  %this.addUser(%blid);
}

function GlassLiveRoom::onUserLeave(%this, %blid, %reason) {
  %chatroom = %this.window;

  %this.removeUser(%blid);

  switch(%reason) {
    case 0:
      %text = "Left";

    case 1:
      %text = "Disconnected";

    case 2:
      %text = "Connection Dropped";

    default:
      %text = "no reason";
  }

  %user = "BLID_" @ %blid; // todo local caching
  %text = "<font:verdana:12><color:666666>" @ %user @ " left the room. [" @ %text @ "]";
  %this.pushText(%text);
}

function GlassLiveRoom::sendMessage(%this, %msg) {
  %obj = JettisonObject();
  %obj.set("type", "string", "roomChat");
  %obj.set("message", "string", %msg);
  %obj.set("room", "string", %this.id);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
}

function GlassLiveRoom::sendCommand(%this, %msg) {
  %obj = JettisonObject();
  %obj.set("type", "string", "roomCommand");
  %obj.set("message", "string", %msg);
  %obj.set("room", "string", %this.id);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
}

function GlassLiveRoom::pushText(%this, %msg) {
  %chatroom = %this.window;
  %val = %chatroom.chattext.getValue();
  if(%val !$= "")
    %val = %val @ "<br>" @ %msg;
  else
    %val = %msg;

  %chatroom.chattext.setValue(%val);
  %chatroom.chattext.forceReflow();
  %chatroom.scrollSwatch.verticalMatchChildren(0, 2);
  %chatroom.scrollSwatch.setVisible(true);
  %chatroom.scroll.scrollToBottom();
}

function GlassLiveRoom::renderUserList(%this) {
  %userSwatch = %this.window.userswatch;
  %userSwatch.deleteAll();
  echo("us: " @ %userSwatch);
  for(%i = 0; %i < %this.getCount(); %i++) {
    %user = %this.getUser(%i);

    if(%user.isAdmin()) {
      %icon = "crown_gold";
      %pos = "2 2";
      %ext = "14 14";
    } else if(%user.isMod()) {
      %icon = "crown_silver";
      %pos = "2 2";
      %ext = "14 14";
    } else {
      %icon = "user";
      %pos = "1 1";
      %ext = "16 16";
    }

    // TODO GuiBitmapButtonCtrl
    %swatch = new GuiSwatchCtrl() {
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "3 3";
      extent = "109 18";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      color = "0 0 0 0";
    };

    %swatch.icon = new GuiBitmapCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      extent = %ext;
      position = %pos;
      bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %icon;
    };

    %swatch.text = new GuiTextCtrl() {
      profile = "GlassFriendTextProfile";
      text = %user.username;
      extent = "45 18";
      position = "22 10";
    };

    %swatch.add(%swatch.icon);
    %swatch.add(%swatch.text);
    %swatch.text.centerY();
    if(%last !$= "") {
      %swatch.placeBelow(%last, 2);
    }
    %last = %swatch;
    %userSwatch.add(%swatch);
  }
  %userSwatch.verticalMatchChildren(0, 5);
  %userSwatch.setVisible(true);
}
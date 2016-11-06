$Glass::Disconnect["Left"] = 0; // [Left]
$Glass::Disconnect["Manual"] = 1; // [Disconnected]
//$Glass::Disconnect["Kicked"] = 2; // [Kicked]
$Glass::Disconnect["Quit"] = 3; // [Quit]
$Glass::Disconnect["Update"] = 4; // [Updates]

function GlassLive::connectToServer() {
  cancel(GlassLive.reconnect);

  GlassFriendsGui_HeaderText.setText("<just:center><font:verdana:22><color:e74c3c>Disconnected");
  if(GlassFriendsGui_HeaderText.isAwake()) {
    GlassFriendsGui_HeaderText.forceReflow();
    GlassFriendsGui_HeaderText.forceCenter();
  }

  if(!GlassLive.ready) {
    glassMessageBoxOk("Wait", "You haven't fully authed yet!");
    return;
  }

  if(GlassLive.connectionTries > 4) {
    %minutes = 5;
    GlassLive.reconnect = GlassLive.schedule((%minutes * 60 * 1000) | 0, connectToServer);
    GlassLive.connectionTries = 0;
    return;
  }

  %server = Glass.liveAddress;
  %port = Glass.livePort;

  //warn("Connecting to notification server...");

  if(isObject(GlassLiveConnection)) {
    if(GlassLiveConnection.connected) {
      error("GlassLiveConnection exists!");
      return;
    }
  } else {
    new TCPObject(GlassLiveConnection) {
      debug = true;
    };
  }

  %this.connected = false;

  GlassLive::setPowerButton(0);

  GlassFriendsGui_HeaderText.setText("<just:center><font:verdana:22><color:e67e22>Connecting...");
  if(GlassFriendsGui_HeaderText.isAwake()) {
    GlassFriendsGui_HeaderText.forceReflow();
    GlassFriendsGui_HeaderText.forceCenter();
  }

  GlassLiveConnection.connect(%server @ ":" @ %port);
}

function GlassLiveConnection::onConnected(%this) {
  GlassLive::setPowerButton(1);

  GlassLive.noReconnect = false;
  GlassLive.connectionTries = 0;
  GlassLive.hideFriendRequests = false;
  GlassLive.hideFriends = false;

  %this.connected = true;
  %obj = JettisonObject();
  %obj.set("type", "string", "auth");
  %obj.set("ident", "string", GlassAuth.ident);
  %obj.set("blid", "string", getNumKeyId());
  %obj.set("version", "string", Glass.version);

  %this.send(jettisonStringify("object", %obj) @ "\r\n");

  GlassFriendsGui_HeaderText.setText("<just:center><font:verdana:22><color:2ecc71>Authenticating...");
  if(GlassFriendsGui_HeaderText.isAwake()) {
    GlassFriendsGui_HeaderText.forceReflow();
    GlassFriendsGui_HeaderText.forceCenter();
  }

  GlassLive.schedule(500, checkPendingFriendRequests);
}

function GlassLiveConnection::onDisconnect(%this) {
  GlassLive::setPowerButton(0);
  GlassFriendsGui_InfoSwatch.color = "210 210 210 255";
  GlassLive_StatusPopUp.setVisible(false);

  %this.connected = false;

  if(!GlassLive.noReconnect)
    GlassLive.reconnect = GlassLive.schedule(5000+getRandom(0, 1000), connectToServer);

  GlassFriendsGui_HeaderText.setText("<just:center><font:verdana:22><color:e74c3c>Disconnected");
  if(GlassFriendsGui_HeaderText.isAwake()) {
    GlassFriendsGui_HeaderText.forceReflow();
    GlassFriendsGui_HeaderText.forceCenter();
  }

  GlassLive::cleanUp();
}

function GlassLiveConnection::onDNSFailed(%this) {
  GlassLive::setPowerButton(0);

  GlassFriendsGui_HeaderText.setText("<just:center><font:verdana:22><color:e74c3c>DNS Failed");
  if(GlassFriendsGui_HeaderText.isAwake()) {
    GlassFriendsGui_HeaderText.forceReflow();
    GlassFriendsGui_HeaderText.forceCenter();
  }

  %this.connected = false;
  GlassLive.connectionTries++;

  if(!GlassLive.noReconnect)
    GlassLive.reconnect = GlassLive.schedule(5000+getRandom(0, 1000), connectToServer);
}

function GlassLiveConnection::onConnectFailed(%this) {
  GlassLive::setPowerButton(0);
  GlassFriendsGui_HeaderText.setText("<just:center><font:verdana:22><color:e74c3c>Connect Failed");

  if(GlassFriendsGui_HeaderText.isAwake()) {
    GlassFriendsGui_HeaderText.forceReflow();
    GlassFriendsGui_HeaderText.forceCenter();
  }

  %this.connected = false;
  GlassLive.connectionTries++;

  if(!GlassLive.noReconnect)
    GlassLive.reconnect = GlassLive.schedule(5000+getRandom(0, 1000), connectToServer);
}

function GlassLiveConnection::doDisconnect(%this, %reason) {
  %obj = JettisonObject();
  %obj.set("type", "string", "disconnect");
  %obj.set("reason", "string", %reason);
  %this.send(jettisonStringify("object", %obj) @ "\r\n");
  %obj.delete();

  %this.disconnect();
  %this.connected = false;

  GlassLive::setPowerButton(0);
  GlassFriendsGui_InfoSwatch.color = "210 210 210 255";
  GlassLive_StatusPopUp.setVisible(false);

  GlassFriendsGui_HeaderText.setText("<just:center><font:verdana:22><color:e74c3c>Disconnected");
  if(GlassFriendsGui_HeaderText.isAwake()) {
    GlassFriendsGui_HeaderText.forceReflow();
    GlassFriendsGui_HeaderText.forceCenter();
  }
}

function GlassLiveConnection::placeCall(%this, %call) {
  %obj = JettisonObject();
  %obj.set("type", "string", %call);
  %this.send(jettisonStringify("object", %obj) @ "\r\n");
  %obj.delete();
}

function GlassLiveConnection::onLine(%this, %line) {
  Glass::debug(%line);
  %error = jettisonParse(%line);
  if(%error) {
    Glass::debug("Parse error - " @ $JSON::Error);
    return;
  }

  %data = $JSON::Value;

  switch$(%data.value["type"]) {
    case "auth":
      // echo("Auth call!");
      switch$(%data.status) {
        case "failed":
          %this.doDisconnect();
          echo("Glass Live Authentication: FAILED");
          if(%data.action $= "reident") {
            GlassAuth.ident = "";
            GlassAuth.heartbeat();
          }

          if(%data.timeout < 5000)
            %data.timeout = 5000;

          GlassLive.reconnect = GlassLive.schedule(%data.timeout+getRandom(0, 1000), connectToServer);

        case "success":
          echo("Glass Live Authentication: SUCCESS");
          GlassLive.onAuthSuccess();

        default:
          echo("\c2Glass Live received an unknown auth response: " @ %data.status);
      }
      // TODO handle failure

    case "notification":
      %title = %data.title;
      %text = %data.text;
      %image = %data.image;
      %sticky = (%data.duration == 0);

      GlassNotificationManager::newNotification(%title, %text, %image, %sticky, %callback);

    case "message":
      %user = GlassLiveUser::getFromBlid(%data.sender_id);

      if(!%user.canSendMessage())
        return;

      %sender = getASCIIString(%data.sender);

      GlassLive::onMessage(%data.message, %sender, %data.sender_id);

      if(GlassSettings.get("Live::MessageNotification"))
        GlassNotificationManager::newNotification(%sender, %data.message, "comment", 0);

      if(GlassSettings.get("Live::MessageSound"))
        alxPlay(GlassUserMsgReceivedAudio);

    case "messageNotification":
      // TODO create GlassLiveUser ? data.chat_username is sent now
      GlassLive::onMessageNotification(%data.message, %data.chat_blid);

    case "roomJoinAuto":
      // TODO just mimic roomJoin for now
      if(GlassSettings.get("Live::RoomNotification"))
        GlassNotificationManager::newNotification("Joined Room", "You've joined " @ %data.title, "add", 0);

      %room = GlassLiveRooms::create(%data.id, %data.title);

      %clients = %data.clients;
      for(%i = 0; %i < %clients.length; %i++) {
        %cl = %clients.value[%i];

        %uo = GlassLiveUser::create(%cl.username, %cl.blid);
        %uo.setStatus(%cl.status);
        %uo.setIcon(%cl.icon);

        %uo.setAdmin(%cl.admin);
        %uo.setMod(%cl.mod);

        if(%cl.blid < 0)
          %uo.setBot(true);

        %room.addUser(%uo.blid);
      }

      %room.createView();

      %motd = %data.motd;
      %motd = strreplace(%motd, "\n", "<br> * ");
      %motd = strreplace(%motd, "[name]", $Pref::Player::NetName);
      %motd = strreplace(%motd, "[vers]", Glass.version);
      %motd = strreplace(%motd, "[date]", getWord(getDateTime(), 0));
      %motd = strreplace(%motd, "[time]", getWord(getDateTime(), 1));

      %motd = "<font:verdana bold:12><color:666666> * " @ %motd;

      %room.pushText(%motd);

      %room.view.userSwatch.getGroup().scrollToTop();

    case "roomJoin":
      if(GlassSettings.get("Live::RoomNotification"))
        GlassNotificationManager::newNotification("Joined Room", "You've joined " @ %data.title, "add", 0);

      %room = GlassLiveRooms::create(%data.id, %data.title);

      %clients = %data.clients;
      for(%i = 0; %i < %clients.length; %i++) {
        %cl = %clients.value[%i];

        %uo = GlassLiveUser::create(%cl.username, %cl.blid);
        %uo.setStatus(%cl.status);
        %uo.setIcon(%cl.icon);

        %uo.setAdmin(%cl.admin);
        %uo.setMod(%cl.mod);

        if(%cl.blid < 0)
          %uo.setBot(true);

        %room.addUser(%uo.blid);
      }

      %room.createView();

      %motd = %data.motd;
      %motd = strreplace(%motd, "\n", "<br> * ");
      %motd = strreplace(%motd, "[name]", $Pref::Player::NetName);
      %motd = strreplace(%motd, "[vers]", Glass.version);
      %motd = strreplace(%motd, "[date]", getWord(getDateTime(), 0));
      %motd = strreplace(%motd, "[time]", getWord(getDateTime(), 1));

      %motd = "<font:verdana bold:12><color:666666> * " @ %motd;

      %room.pushText(%motd);

      %room.view.userSwatch.getGroup().scrollToTop();

    case "messageTyping":
      %user = GlassLiveUser::getFromBlid(%data.sender);

      if(!%user.canSendMessage())
        return;

      GlassLive::setMessageTyping(%data.sender, %data.typing);

    case "roomMessage":
      %room = GlassLiveRoom::getFromId(%data.room);
      if(isObject(%room)) {
        %msg = %data.msg;
        %sender = %data.sender;
        %senderblid = %data.sender_id;

        %senderUser = GlassLiveUser::getFromBlid(%senderblid);
        
        if(%senderUser.isBlocked())
          return;

        %room.pushMessage(%senderUser, %msg, %data);
      }

    case "roomText":
      %room = GlassLiveRoom::getFromId(%data.id);

      if(isObject(%room)) {
        %data.text = strreplace(%data.text, "[name]", $Pref::Player::NetName);
        %data.text = strreplace(%data.text, "[vers]", Glass.version);
        %data.text = strreplace(%data.text, "[date]", getWord(getDateTime(), 0));
        %data.text = strreplace(%data.text, "[time]", getWord(getDateTime(), 1));

        %room.pushText(%data.text);
      }

    case "roomUserJoin":
      %uo = GlassLiveUser::create(%data.username, %data.blid);
      %uo.setAdmin(%data.admin);
      %uo.setMod(%data.mod);
      if(%uo.blid < 0) {
        %uo.setBot(true);
      }
      %uo.setStatus(%data.status);
      %uo.setIcon(%data.icon);

      %room = GlassLiveRoom::getFromId(%data.id);
      if(isObject(%room))
        %room.onUserJoin(%uo.blid);

    case "roomUserLeave": //other user got removed
      %room = GlassLiveRoom::getFromId(%data.id);
      if(isObject(%room))
        %room.onUserLeave(%data.blid, %data.reason);

    case "roomUserStatus":
      %uo = GlassLiveUser::getFromBlid(%data.blid);
      %uo.setStatus(%data.status);
      %room = GlassLiveRoom::getFromId(%data.id);
      if(isObject(%room))
        %room.renderUserList();

    case "roomUserIcon":
      %uo = GlassLiveUser::getFromBlid(%data.blid);
      %uo.setIcon(%data.icon, %data.id);

    case "roomKicked": //we got removed from a room
      warn("TODO: roomKicked for reason " @ %data.reason);
      glassMessageBoxOk("Kicked", "You've been kicked from -room name-:<br><br>" @ %data.reason);

    case "roomBanned": //we got banned from a room
      if(%data.all) {
        //we got banned from all rooms
        glassMessageBoxOk("Banned", "You've been banned from all chatrooms for <font:verdana bold:13>" @ %data.duration @ "<font:verdana:13> seconds:<br><br><font:verdana bold:13>" @ %data.reason);
      } else {
        %room = GlassLiveRoom::getFromId(%data.id);
        glassMessageBoxOk("Banned", "You've been banned from <font:verdana bold:13>" @ %room.name @ "<font:verdana:13> for <font:verdana bold:13>" @ %data.duration @ "<font:verdana:13> seconds:<br><br><font:verdana bold:13>" @ %data.reason);
      }


    // case "roomAwake":
      // %room = GlassLiveRoom::getFromId(%data.id);
      // if(isObject(%room))
        // %room.setUserAwake(%data.user, %data.awake);

    case "roomList":
      %window = GlassLive.pendingRoomList;
      %window.openRoomBrowser(%data.rooms);

    case "friendsList":
      for(%i = 0; %i < %data.friends.length; %i++) {
        %friend = %data.friends.value[%i];
        %uo = GlassLiveUser::create(%friend.username, %friend.blid);
        %uo.setFriend(true);
        %uo.setStatus(%friend.status);
        %uo.setIcon(%friend.icon);

        GlassLive::addFriendToList(%uo);
      }
      GlassLive::createFriendList();


    case "friendRequests":
      for(%i = 0; %i < %data.requests.length; %i++) {
        %friend = %data.requests.value[%i];
        %uo = GlassLiveUser::create(%friend.username, %friend.blid);

        %uo.setFriendRequest(true);

        GlassLive::addfriendRequestToList(%uo);
      }

      if(%data.requests.length == 0)
        GlassLive.friendRequestList = "";

      GlassLive::createFriendList();

    case "friendRequest":
      if(strstr(GlassLive.friendRequestList, %blid = %data.sender_blid) == -1) {
        %username = %data.sender;
        %uo = GlassLiveUser::create(%username, %blid);

        GlassLive::addfriendRequestToList(%uo);

        GlassLive::createFriendList();

        GlassNotificationManager::newNotification("Friend Request", "You've been sent a friend request by <font:verdana bold:13>" @ %uo.username @ " (" @ %uo.blid @ ")", "email_add", 0);

        alxPlay(GlassFriendRequestAudio);
      }

    case "friendStatus":
      GlassLive.schedule(100, friendOnline, %data.blid, %data.status);

    case "friendIcon":
      %uo = GlassLiveUser::getFromBlid(%data.blid);
      %uo.setIcon(%data.icon);

    case "friendAdd": // create all-encompassing ::addFriend function for this?
      %uo = GlassLiveUser::create(%data.username, %data.blid);
      %uo.setFriend(true);
      %uo.setStatus(%data.status);
      %uo.setIcon(%data.icon);

      GlassLive::removeFriendRequestFromList(%uo.blid);
      GlassLive::addFriendToList(%uo);

      GlassLive::createFriendList();

      if(isObject(%room = GlassChatroomWindow.activeTab.room))
        %room.renderUserList();

      GlassNotificationManager::newNotification("Friend Added", "<font:verdana bold:13>" @ %uo.username @ " (" @ %uo.blid @ ") <font:verdana:13>has been added to your friends list.", "user_add", 0);

      alxPlay(GlassFriendAddedAudio);

    case "friendRemove":
      %uo = GlassLiveUser::getFromBlid(%data.blid);

      GlassLive::removeFriend(%data.blid, true);

      GlassNotificationManager::newNotification("Friend Removed", "<font:verdana bold:13>" @ %uo.username @ " (" @ %uo.blid @ ") <font:verdana:13>has been removed from your friends list.", "user_delete", 0);

      alxPlay(GlassFriendRemovedAudio);

    case "groupJoin":
      %group = GlassLiveGroup::create(%data.id, %data.clients);
      %group.createGui();

    case "groupInvite":
      %id = %data.id;
      %name = %data.inviterName;
      %blid = %data.inviterBlid;
      %users = %data.users;

      GlassNotificationManager::newNotification("Groupchat Invite", "You've been invited to group chat by <font:verdana bold:13>" @ %name, "group", 1, "GlassLive::joinGroupPrompt(" @ %id @ ");");

    case "groupMessage":
      %group = GlassLiveGroup::getFromId(%data.id);

      %name = %data.senderName;
      %blid = %data.senderBlid;

      %user = GlassLiveUser::getFromBlid(%blid);
      %group.pushMessage(%user, %data.msg);

    case "groupClientEnter":
      %client = GlassLiveUser::create(%data.username, %data.blid);
      %group = GlassLiveGroup::getFromId(%data.id);

      %group.addUser(%client.blid);
      %group.pushText("<font:verdana:12><color:666666>" @ %client.username @ " entered the group.");

    case "groupClientLeave":
      %group = GlassLiveGroup::getFromId(%data.id);
      %group.removeUser(%data.blid);

    case "location":
      GlassLive::displayLocation(%data);

    case "serverListUpdate":
      return;
      GlassServerList.doLiveUpdate(%data.ip, %data.port, %data.key, %data.value);

    case "serverListing":
      return;
      GlassServerList.doLiveUpdate(getWord(%data.addr, 0), getWord(%data.addr, 1), "hasGlass", %data.hasGlass);


    case "blockedList":
      %list = "";
      for(%i = 0; %i < %data.blocked.length; %i++) {
        %userData = %data.blocked.value[%i];
        // echo("blocked: " @ %userData.blid);
        %list = %list SPC %userData.blid;

        %user = GlassLiveUser::create(%userData.username, %userData.blid);
        %user.blocked = true;
      }

      if(strlen(%list) > 0)
        %list = getSubStr(%list, 1, strlen(%list)-1);

      GlassLive.blockedList = %list;
      GlassLive::createFriendList();

    case "messageBox":
      glassMessageBoxOk(%data.title, %data.text);

    case "shutdown":
      %planned = %data.planned;
      %reason = %data.reason;
      %timeout = %data.timeout;

      if(%timeout < 5000) {
        %timeout = 5000;
      }

      GlassNotificationManager::newNotification("Glass Live" SPC (%planned ? "Planned" : "Unplanned") SPC "Shutdown", "Reason:" SPC %reason, "roadworks", 1);

      alxPlay(GlassBellAudio);

      %this.disconnect();
      %this.connected = false;
      GlassLive.reconnect = GlassLive.schedule(%timeout+getRandom(0, 2000), connectToServer);

    case "disconnected":
      // 0 - server shutdown
      // 1 - other sign-in
      // 2 - barred
      if(%data.reason == 1) {
        GlassLive.noReconnect = true;
        glassMessageBoxOk("Disconnected", "You logged in from somewhere else!");
        %this.disconnect();
      } else if(%data.reason == 2) {
        GlassLive.noReconnect = true;
        glassMessageBoxOk("Disconnected", "You are barred from using Glass Live!<br><br>Sorry for the inconvenience.");
        GlassSettings.update("Live::StartupConnect", false);
        %this.disconnect();
      }

    case "kicked": //we got kicked from all service
      GlassLive.noReconnect = true;
      glassMessageBoxOk("Kicked", "You've been kicked from Glass Live:<br><br>\"" @ %data.reason @ "\"<br><br>Sorry for the inconvenience.");
      %this.disconnect();

    case "barred": //we're not allowed to use glass live
      GlassLive.noReconnect = true;
      glassMessageBoxOk("Barred", "You've been barred from all Glass Live service for " @ %data.duration @ " seconds:<br><br>\"" @ %data.reason @ "\"<br><br>Sorry for the inconvenience.");
      GlassSettings.update("Live::StartupConnect", false);
      %this.disconnect();

    case "error":
      if(%data.showDialog) {
        glassMessageBoxOk("Glass Live Error", %data.message);
      } else {
        echo("Glass Live Error: " @ %data.message);
      }
  }
  //%data.delete();
}

function formatTimeHourMin(%datetime) {
  %t = getWord(%datetime, 0);
  %t = getSubStr(%t, 0, strpos(%t, ":", 4));
  return %t SPC getWord(%datetime, 1);
}

package GlassLiveConnectionPackage {
  function messageCallback(%this, %call) {
    if(updater.restartRequired) {
      if(%call $= "quit();") {
        GlassLive::disconnect($Glass::Disconnect["Update"]);
      }
    }
    parent::messageCallback(%this, %call);
  }

  function authTCPobj_Client::onDisconnect(%this) {
    parent::onDisconnect(%this);

    if(GlassLiveConnection.connected) {
      GlassLive::disconnect($Glass::Disconnect["Manual"]);
    }

    GlassAuth.schedule(0, init);
    GlassAuth.schedule(100, heartbeat);
  }
};
activatePackage(GlassLiveConnectionPackage);

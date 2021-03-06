function GlassServerControlC::init() {
  if(!isObject(GlassServerControlC))
    new ScriptObject(GlassServerControlC) {
      enabled = false;
    };

  GlassServerControlC::setTab(2);
}

$remapDivision[$remapCount] = "Blockland Glass";
  $remapName[$remapCount] = "Server Settings";
  $remapCmd[$remapCount] = "openGlassSettings";
  $remapCount++;

function openGlassSettings(%down) {
  if(!%down) {
    if(GlassServerControlGui.isAwake()) {
      canvas.popDialog(GlassServerControlGui);
    } else if(GlassServerControlC.enabled) {
      canvas.pushDialog(GlassServerControlGui);
    } else {
      if(ServerConnection.hasGlass) {
        glassMessageBoxOk("Glass Server Preferences", "You do not have access to this server's preferences.");
      } else {
        glassMessageBoxOk("Glass Server Preferences", "Blockland Glass is not running on this server.");
      }
    }
  }
}

function GlassServerControlC::setTab(%tab) {
  for(%i = 0; %i < 3; %i++) {
    %ctrl = "GlassServerControlGui_Pane" @ %i+1;
    %button = "GlassServerControlGui_Tab" @ %i+1;
    if(%i+1 == %tab) {
      %ctrl.setVisible(true);
      %button.mColor = "84 217 140 120";
    } else {
      %ctrl.setVisible(false);
      %button.mColor = "255 255 255 150";
    }
  }

  GlassServerControlGui_CursorFix.makeFirstResponder(true);
}

function GlassServerControlC::openCategory(%category) {
  if(isObject(GlassServerControlGui.openCategory)) {
    %s = GlassServerControlGui.openCategory.cat.sw;
    %s.color = %s.ocolor;
    %s.selected = false;
    GlassServerControlGui.openCategory.setVisible(false);
  }

  %obj = "GlassServerControlGui_Pref" @ %category.id;
  if(!isObject(%obj)) return;

  %obj.setVisible(true);

  %category.sw.selected = true;
  %category.sw.color = "131 195 243 150";

  GlassServerControlGui.openCategory = %obj;

  GlassServerControl_PrefScroll.extent = %obj.extent;

  GlassServerControl_PrefScroll.getGroup().scrollToTop();
  GlassServerControl_PrefScroll.setVisible(true);
}

function GlassServerControlC::renderAll() {
  GlassServerControl_PrefScroll.deleteAll();
  GlassServerControlGui_Prefs_Categories.deleteAll();

  %tabLast = "";
  %odd = 0;
  for(%i = 0; %i < GlassPrefGroup.getCount(); %i++) {
    %category = GlassPrefGroup.getObject(%i);

    %tab = GlassServerControlC::createCategoryTab(%category, %odd = !%odd);
    %category.sw = %tab;

    GlassServerControlGui_Prefs_Categories.add(%tab);

    if(%i > 0) {
      %tab.placeBelow(%tabLast);
    }
    %tabLast = %tab;

    GlassServerControlC::renderCategory(%category);
  }

  GlassServerControlGui_Prefs_Categories.verticalMatchChildren(0, 0);
  GlassServerControlGui_Prefs_Categories.setVisible(true);
  GlassServerControl_PrefScroll.setVisible(true);

  GlassServerControlC::openCategory(GlassPrefGroup.getObject(0));
}

function GlassServerControlC::renderCategory(%category) {
  %parent = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "0 0";
    extent = getWord(GlassServerControl_PrefScroll.extent, 0) SPC 0;
    minExtent = "8 2";
    enabled = "1";
    visible = "0";
    clipToParent = "1";
    color = "0 0 0 0";
    cat = %category;
  };

  %subheader = %category.getObject(0).subcategory;
  %header = GlassServerControlC::createHeader(%subheader);
  %header.position = 0 SPC 0;
  %parent.add(%header);
  %currentY += 24;

  for(%j = 0; %j < %category.getCount(); %j++) {
    %pref = %category.getObject(%j);
    if(%pref.subcategory !$= %subheader) {
      %subheader = %pref.subcategory;
      %header = GlassServerControlC::createHeader(%subheader);
      %parent.add(%header);
      %header.position = 0 SPC %currentY;
      %currentY += 24;
    }
    %swatch = "";
    switch$(%pref.type) {
      case "bool":
        %swatch = GlassServerControlC::createCheckbox();
        %swatch.text.setText(%pref.title);
        %swatch.ctrl.setValue(%pref.value);

      case "num":
        %swatch = GlassServerControlC::createInt();
        %swatch.text.setText(%pref.title);
        %swatch.ctrl.setValue(%pref.value);

      case "slider":
        %swatch = GlassServerControlC::createSlider();
        %swatch.text.setText(%pref.title);
        %swatch.ctrl.setValue(%pref.value);
        %swatch.ctrl.range = %pref.parm;

      case "string":
        %swatch = GlassServerControlC::createText();
        %swatch.text.setText(%pref.title);
        %swatch.ctrl.setValue(expandEscape(%pref.value));

      case "textarea":
        %swatch = GlassServerControlC::createTextArea();
        %swatch.text.setText(%pref.title);
        %swatch.ctrl.setValue(%pref.value);

      case "dropdown":
        %swatch = GlassServerControlC::createList();
        %swatch.text.setText(%pref.title);
        %options = %pref.params;
        for(%k = 0; %k < getWordCount(%options); %k += 2) {
          %swatch.ctrl.add(strreplace(getWord(%options, %k), "_", " "), getWord(%options, %k+1));
        }
        %swatch.ctrl.setSelected(%pref.value);

      case "playercount":
        %swatch = GlassServerControlC::createList();
        %swatch.text.setText(%pref.title);
        %options = %pref.params;
        for(%k = 0; %k < 99; %k++) {
          %swatch.ctrl.add(%k+1, %k+1);
        }
        %swatch.ctrl.setSelected(%pref.value);

      case "wordlist":
        %swatch = GlassServerControlC::createText();
        %swatch.text.setText(%pref.title);
        %swatch.ctrl.setValue(expandEscape(%pref.value));

      case "userlist": // these should be done at some point
        %swatch = "unfinished";

      case "button":
        %swatch = "unfinished";

      case "rgb":
        %swatch = GlassServerControlC::createRGB();
        %swatch.text.setText(%pref.title);

        %alpha = %pref.params;
        if(%alpha) {
          %swatch.preview.color = %pref.value;
        } else {
          %swatch.preview.color = getWords(%pref.value, 0, 3) SPC 255;
        }

      case "colorset":
        %swatch = "unfinished";

      case "datablock":
        %swatch = "unfinished";

      case "datablocklist":
        %swatch = "unfinished";
    }

    if(!isObject(%swatch)) {
      if(%swatch !$= "unfinished") {
        warn("Failed to make pref of type \"" @ %pref.type @ "\"");
      }
      continue;
    }

    if(%odd) {
      %swatch.color = "220 220 220 255";
    } else {
      %swatch.color = "230 230 230 255";
    }

    %odd = !%odd;

    %swatch.ctrl.command = "GlassServerControlC::valueUpdate(" @ %swatch.getId() @ ");";
    %swatch.position = 0 SPC %currentY;
    %parent.add(%swatch);

    %pref.swatch = %swatch;
    %swatch.pref = %pref;

    if(%pref.type !$= "textarea") {
      %currentY += 32;
    } else {
      %currentY += 129;
    }
  }

  %parent.verticalMatchChildren(123, 0);
  %category.tab = %parent;

  %parent.setName("GlassServerControlGui_Pref" @ %category.id);
  GlassServerControl_PrefScroll.add(%parent);
}

function GlassServerControlC::createCategoryTab(%cat, %odd) {
  if(%odd) {
    %color = "230 230 230 255";
  } else {
    %color = "235 235 235 255";
  }

  %swat = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = 0 SPC 0;
    extent = "135 24";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = %color;
    ocolor = %color;
    rcolor = %color;
    cat = %cat;
  };

  %swat.bitmap = new GuiBitmapCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "4 4";
    extent = "16 16";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %cat.icon;
    wrap = "0";
    lockAspectRatio = "0";
    alignLeft = "0";
    alignTop = "0";
    overflowImage = "0";
    keepCached = "0";
    mColor = "255 255 255 255";
    mMultiply = "0";
  };

  %swat.text = new GuiTextCtrl() {
    profile = "GuiTextVerdanaProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "24 3";
    extent = "38 18";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = %cat.name;
    maxLength = "255";
  };

  %swat.mouse = new GuiMouseEventCtrl(GlassServerControlGui_CatMouseCtrl) {
    category = %cat;
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "0 0";
    extent = "135 24";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    lockMouse = "0";
  };

  %swat.add(%swat.bitmap);
  %swat.add(%swat.text);
  %swat.add(%swat.mouse);
  return %swat;
}

function GlassServerControlC::createHeader(%text) {
  return new GuiSwatchCtrl() {
     profile = "GuiDefaultProfile";
     horizSizing = "right";
     vertSizing = "bottom";
     position = "1 1";
     extent = "430 24";
     minExtent = "8 2";
     enabled = "1";
     visible = "1";
     clipToParent = "1";
     color = "84 217 140 255";

     new GuiMLTextCtrl() {
        profile = "GuiMLTextProfile";
        horizSizing = "center";
        vertSizing = "center";
        position = "0 4";
        extent = "430 16";
        minExtent = "325 2";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        lineSpacing = "2";
        allowColorChars = "0";
        maxChars = "-1";
        text = "<just:center><font:verdana bold:14>" @ %text;
        maxBitmapHeight = "-1";
        selectable = "1";
        autoResize = "1";
     };
  };
}

function GlassServerControlC::createTextArea() {
  %swatch = new GuiSwatchCtrl() {
     profile = "GuiDefaultProfile";
     horizSizing = "right";
     vertSizing = "bottom";
     position = "1 25";
     extent = "325 128";
     minExtent = "8 2";
     enabled = "1";
     visible = "1";
     clipToParent = "1";
     color = "100 100 100 50";
  };

  %swatch.text = new GuiTextCtrl() {
    profile = "GuiTextVerdanaProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 4";
    extent = "77 18";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "Some checkbox";
    maxLength = "255";
  };

  %swat2 = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "5 25";
    extent = "270 98";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "255 255 255 200";
  };

  %swatch.ctrl = new GuiMLTextEditCtrl() {
    profile = "GuiMLTextEditProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "5 5";
    extent = "260 93";
    minExtent = "260 93";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    lineSpacing = "2";
    allowColorChars = "0";
    maxChars = "-1";
    maxBitmapHeight = "-1";
    selectable = "1";
    autoResize = "1";
  };

  %swatch.add(%swatch.text);
  %swatch.add(%swat2);
  %swat2.add(%swatch.ctrl);
  return %swatch;
}

function GlassServerControlC::createList() {
  %swatch = new GuiSwatchCtrl() {
     profile = "GuiDefaultProfile";
     horizSizing = "right";
     vertSizing = "bottom";
     position = "1 25";
     extent = "430 32";
     minExtent = "8 2";
     enabled = "1";
     visible = "1";
     clipToParent = "1";
     color = "100 100 100 50";
  };

  %swatch.text = new GuiTextCtrl() {
    profile = "GuiTextVerdanaProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "10 7";
    extent = "77 18";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "";
    maxLength = "255";
  };

  %swatch.ctrl = new GuiPopUpMenuCtrl() {
    profile = "GuiPopUpMenuProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "320 6";
    extent = "100 20";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    maxLength = "255";
    maxPopupHeight = "200";
  };

  %swatch.add(%swatch.text);
  %swatch.add(%swatch.ctrl);
  return %swatch;
}

function GlassServerControlC::createCheckbox() {
  %swatch = new GuiSwatchCtrl() {
     profile = "GuiDefaultProfile";
     horizSizing = "right";
     vertSizing = "bottom";
     position = "1 25";
     extent = "430 32";
     minExtent = "8 2";
     enabled = "1";
     visible = "1";
     clipToParent = "1";
     color = "100 100 100 50";
  };

  %swatch.text = new GuiTextCtrl() {
    profile = "GuiTextVerdanaProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "10 7";
    extent = "77 18";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "";
    maxLength = "255";
  };

  %swatch.ctrl = new GuiCheckBoxCtrl() {
    profile = "GuiCheckBoxProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "408 8";
    extent = "16 16";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    groupNum = "-1";
    buttonType = "ToggleButton";
    text = " ";
  };

  %swatch.add(%swatch.text);
  %swatch.add(%swatch.ctrl);
  return %swatch;
}

function GlassServerControlC::createInt() {
  %swatch = new GuiSwatchCtrl() {
     profile = "GuiDefaultProfile";
     horizSizing = "right";
     vertSizing = "bottom";
     position = "1 25";
     extent = "430 32";
     minExtent = "8 2";
     enabled = "1";
     visible = "1";
     clipToParent = "1";
     color = "100 100 100 50";
  };

  %swatch.text = new GuiTextCtrl() {
    profile = "GuiTextVerdanaProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "10 7";
    extent = "77 18";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "Some checkbox";
    maxLength = "255";
  };

  %swatch.ctrl = new GuiTextEditCtrl() {
    profile = "GlassTextEditProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "350 7";
    extent = "70 18";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    maxLength = "255";
    historySize = "0";
    password = "0";
    tabComplete = "0";
    sinkAllKeyEvents = "0";
  };

  %swatch.add(%swatch.text);
  %swatch.add(%swatch.ctrl);
  return %swatch;
}

function GlassServerControlC::createSlider() {
  %swatch = new GuiSwatchCtrl() {
     profile = "GuiDefaultProfile";
     horizSizing = "right";
     vertSizing = "bottom";
     position = "1 25";
     extent = "430 32";
     minExtent = "8 2";
     enabled = "1";
     visible = "1";
     clipToParent = "1";
     color = "100 100 100 50";
  };

  %swatch.text = new GuiTextCtrl() {
    profile = "GuiTextVerdanaProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "10 7";
    extent = "77 18";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "Some checkbox";
    maxLength = "255";
  };

  %swatch.ctrl = new GuiSliderCtrl() {
    profile = "GuiSliderProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "280 6";
    extent = "140 20";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    range = "0.000000 1.000000";
    ticks = "10";
    value = "0.433333";
    snap = "0";
  };

  %swatch.add(%swatch.text);
  %swatch.add(%swatch.ctrl);
  return %swatch;
}

function GlassServerControlC::createText() {
  %swatch = new GuiSwatchCtrl() {
     profile = "GuiDefaultProfile";
     horizSizing = "right";
     vertSizing = "bottom";
     position = "1 25";
     extent = "430 32";
     minExtent = "8 2";
     enabled = "1";
     visible = "1";
     clipToParent = "1";
     color = "100 100 100 50";
  };

  %swatch.text = new GuiTextCtrl() {
    profile = "GuiTextVerdanaProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "10 7";
    extent = "77 18";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "Some checkbox";
    maxLength = "255";
  };

  %swatch.ctrl = new GuiTextEditCtrl() {
    profile = "GlassTextEditProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "280 7";
    extent = "140 18";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    maxLength = "255";
    historySize = "0";
    password = "0";
    tabComplete = "0";
    sinkAllKeyEvents = "0";
  };

  %swatch.add(%swatch.text);
  %swatch.add(%swatch.ctrl);
  return %swatch;
}

function GlassServerControlC::createRGB() {
  %swatch = new GuiSwatchCtrl() {
     profile = "GuiDefaultProfile";
     horizSizing = "right";
     vertSizing = "bottom";
     position = "1 25";
     extent = "430 32";
     minExtent = "8 2";
     enabled = "1";
     visible = "1";
     clipToParent = "1";
     color = "100 100 100 50";
  };

  %swatch.text = new GuiTextCtrl() {
    profile = "GuiTextVerdanaProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "10 7";
    extent = "77 18";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "";
    maxLength = "255";
  };

  %swatch.preview = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "408 8";
    extent = "16 16";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "255 0 255 255";
  };

  %swatch.btn = new GuiBorderButtonCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "408 8";
    extent = "16 16";
    command = "GlassServerControlGui::openColorSelector(" @ %swatch.getId() @ ");";
  };

  %swatch.add(%swatch.text);
  %swatch.add(%swatch.preview);
  %swatch.add(%swatch.btn);
  return %swatch;
}

function GlassServerControlC::setEnabled(%this, %enabled) {
  %this.enabled = %enabled;

  if(%enabled) {
    if(!%this.requested) {
      GlassPrefBridge::requestPreferences();
    }
  } else {
    canvas.popDialog(GlassServerControlGui);
    GlassPrefGroup::cleanup();
  }
}

function GlassServerControlC::valueUpdate(%obj) {
  %pref = %obj.pref;
  %type = %pref.type;
  %parm = %pref.params;

  if(%type $= "num") {
    if(%parm !$= "") {
      %val = %obj.ctrl.getValue();

      %min = getWord(%parm, 0);
      %max = getWord(%parm, 1);
      %decimal = getWord(%parm, 2);

      if(%val !$= "") {
        if(%val < %min) {
          %obj.ctrl.setValue(%min);
        } else if(%val > %max) {
          %obj.ctrl.setValue(%max);
        }

        if(%decimal !$= "") {
          if(strpos(%val, ".") != -1) {
            if(strlen(%val) - strpos(%val, ".") > %decimal) {
              %newval = getSubStr(%val, 0, strpos(%val, ".")+%decimal+1);
              %obj.ctrl.setValue(%newval);
            }

            if(%decimal == 0) {
              %newval = getSubStr(%val, 0, strpos(%val, "."));
              %obj.ctrl.setValue(%newval);
            }
          }
        }
      }
    }
  } else if(%type $= "string" || %type $= "textarea") {
    if(%parm !$= "") {
      if(strlen(%obj.ctrl.getValue()) > %parm) {
        %obj.ctrl.setValue(getsubstr(%obj.ctrl.getValue(), 0, %parm));
      }
    }
  }

  if(%type $= "dropdown" || %type $= "playercount") {
    %pref.localvalue = %obj.ctrl.getSelected();
  } else {
    %pref.localvalue = collapseEscape(%obj.ctrl.getValue());
  }
}

function GlassServerControlC::populatePlayerList(%this) {
  %list = GlassServerControlGui_PlayerList;

  %list.addrow("Name\t9789\t-", 0);
}

function GlassServerControlC::onSelect(%list) {
  if(%list == 1) {
    %list = GlassServerControlGui_AdminList;
    %otherlist = GlassServerControlGui_PlayerList;
  } else if(%list == 2) {
    %list = GlassServerControlGui_PlayerList;
    %otherlist = GlassServerControlGui_AdminList;
  }
  %row = %list.getValue();
  %otherlist.setSelectedRow(-1);

  %status = getField(%row, 2);
  %blid = getField(%row, 1);
}

function GlassServerControlC::addUser() {
  %blid = GlassServerControlGui_InputBLID.getValue();

  GlassServerControlGui_InputBLID.setValue("");

  if((%blid+0 !$= %blid) || %blid < 0) {
    glassMessageBoxOk("Invalid BLID", "That is not a valid Blockland ID!");
    return;
  }

  %rank = GlassServerControlGui_InputRank.getValue();

  if(%rank $= "Super Admin") {
    %rank = 2;
  } else if(%rank $= "Admin") {
    %rank = 1;
  }

  if(%rank $= "") {
    return;
  }

  commandToServer('GlassSetAdmin', %blid, %rank, true);
}

function GlassServerControlC::addSelected(%rank, %auto) {
  %list = GlassServerControlGui_PlayerList;
  %row = %list.getValue();

  %status = getField(%row, 2);
  %blid = getField(%row, 1);

  if(%auto) {
    GlassServerControlGui_InputBLID.setValue("");

    GlassServerControlGui_AddUserBLID.setVisible(true);

    if(%blid !$= "") {
      GlassServerControlGui_InputBLID.setValue(%blid);
    }

    GlassServerControlGui_InputRank.setValue("Admin");
    GlassServerControlGui_InputBLID.makeFirstResponder(true);
  } else {
    if(%blid $= "" || %blid $= "LAN") {
      return;
    }

    commandToServer('GlassSetAdmin', %blid, %rank, %auto);
  }
}

function GlassServerControlC::removeSelected() {
  %list = GlassServerControlGui_AdminList;
  %row = %list.getValue();

  %status = getField(%row, 2);
  %blid = getField(%row, 1);

  if(%blid $= "" || %blid $= "LAN") {
    return;
  }

  commandToServer('GlassSetAdmin', %blid, 0, true);
}

function GlassServerControlC::deAdminSelected() {
  %list = GlassServerControlGui_PlayerList;
  %row = %list.getValue();

  %status = getField(%row, 2);
  %blid = getField(%row, 1);

  if(%blid $= "" || %blid $= "LAN") {
    return;
  }

  commandToServer('GlassSetAdmin', %blid, 0, false);
}

function GlassServerControlGui::onWake(%this) {
  GlassServerControlGui_PlayerList.clear();
  for(%i = 0; %i < NPL_List.rowCount(); %i++) {
    %row = NPL_List.getRowText(%i);
    %admin = getField(%row, 0);
    %name = getField(%row, 1);
    %blid = getField(%row, 3);

    GlassServerControlGui_PlayerList.addRow(%i, %name TAB %blid TAB %admin);
  }
  commandToServer('openPrefs');
}

function GlassServerControlGui_AddUserBLID::onWake(%this) {
  GlassServerControlGui_InputRank.clear();

  GlassServerControlGui_InputRank.setValue("Admin");

  GlassServerControlGui_InputRank.add("Super Admin", 1);
  GlassServerControlGui_InputRank.add("Admin", 2);
}

function GlassServerControlGui_CatMouseCtrl::onMouseDown(%this) {
  GlassServerControlC::openCategory(%this.category);
}

function GlassServerControlGui_CatMouseCtrl::onMouseEnter(%this) {
  %swatch = %this.getGroup();
  if(!%swatch.selected)
    %swatch.color = "240 240 240 255";
}

function GlassServerControlGui_CatMouseCtrl::onMouseLeave(%this) {
  %swatch = %this.getGroup();
  if(!%swatch.selected)
    %swatch.color = %swatch.ocolor;
  else
    %swatch.color = "131 195 243 150";
}

function GlassServerControlGui::openColorSelector(%swatch) {
  %pref = %swatch.pref;

  %alpha = %pref.params $= "1";
  GlassServerControlGui_ColorSelectA.enabled = %alpha;
  GlassServerControlGui_ColorTextA.enabled = %alpha;
  GlassServerControlGui_ColorTextA.getGroup().setVisible(%alpha);

  GlassServerControlGui_ColorTextR.setValue(getWord(%pref.localvalue, 0));
  GlassServerControlGui_ColorTextG.setValue(getWord(%pref.localvalue, 1));
  GlassServerControlGui_ColorTextB.setValue(getWord(%pref.localvalue, 2));

  if(%alpha)
    GlassServerControlGui_ColorTextA.setValue(getWord(%pref.localvalue, 3));
  else
    GlassServerControlGui_ColorTextA.setValue(255);

  GlassServerControlGui_ColorSelector.pref = %pref;
  GlassServerControlGui_ColorSelector.alphaLocked = !%alpha;
  GlassServerControlGui_ColorSelector.updateColor(true);
  GlassServerControlGui_ColorSelector.setVisible(true);
  GlassServerControlGui.pushToBack(GlassServerControlGui_ColorSelector);
  GlassServerControlGui_ColorSelector.forceCenter();
}

function GlassServerControlGui_ColorSelector::updateColor(%this, %isText) {
  if(%isText) {
    %color1 = GlassServerControlGui_ColorTextR.getValue();
    %color2 = GlassServerControlGui_ColorTextG.getValue();
    %color3 = GlassServerControlGui_ColorTextB.getValue();
    %color4 = GlassServerControlGui_ColorTextA.getValue();
  } else {
    %color1 = GlassServerControlGui_ColorSelectR.getValue();
    %color2 = GlassServerControlGui_ColorSelectG.getValue();
    %color3 = GlassServerControlGui_ColorSelectB.getValue();
    %color4 = GlassServerControlGui_ColorSelectA.getValue();
  }

  for(%i = 0; %i < 4; %i++) {
    %c = %color[%i+1];

    %pos = strpos(%c, ".");
    if(%pos != -1) {
      %c = getSubStr(%c, 0, %pos);
    }

    if(%c+0 !$= %c || %c < 0) {
      %c = 0;
    } else if(%c > 255) {
      %c = 255;
    }

    %color[%i+1] = %c;
  }

  if(%this.alphaLocked) {
    %color4 = 255;
  }

  %color = %color1 SPC %color2 SPC %color3 SPC %color4;

  GlassServerControlGui_ColorSelectR.setValue(%color1);
  GlassServerControlGui_ColorSelectG.setValue(%color2);
  GlassServerControlGui_ColorSelectB.setValue(%color3);
  GlassServerControlGui_ColorSelectA.setValue(%color4);

  GlassServerControlGui_ColorTextR.setValue(%color1);
  GlassServerControlGui_ColorTextG.setValue(%color2);
  GlassServerControlGui_ColorTextB.setValue(%color3);
  GlassServerControlGui_ColorTextA.setValue(%color4);

  GlassServerControlGui_ColorPreview.color = %color;
}

function GlassServerControlGui_ColorSelector::submitColor(%this) {
  GlassServerControlGui_ColorSelector.setVisible(false);

  %color1 = GlassServerControlGui_ColorSelectR.getValue();
  %color2 = GlassServerControlGui_ColorSelectG.getValue();
  %color3 = GlassServerControlGui_ColorSelectB.getValue();
  %color4 = GlassServerControlGui_ColorSelectA.getValue();

  for(%i = 0; %i < 4; %i++) {
    %c = %color[%i+1];

    %pos = strpos(%c, ".");
    if(%pos != -1) {
      %c = getSubStr(%c, 0, %pos);
    }

    if(%c+0 !$= %c || %c < 0) {
      %c = 0;
    } else if(%c > 255) {
      %c = 255;
    }

    %color[%i+1] = %c;
  }

  if(%this.alphaLocked) {
    %color4 = 255;
  }

  %color = %color1 SPC %color2 SPC %color3 SPC %color4;

  %this.pref.localvalue = %color;
  %this.pref.swatch.preview.color = getWords(%color, 0, 3) SPC 255;
}

function clientCmdGlassAdminListing(%data, %append) {
  if(!%append) {
    GlassServerControlGui_AdminList.clear();
  }

  if(%data !$= "") {
    for(%i = 0; %i < getLineCount(%data); %i++) {
      GlassServerControlGui_AdminList.addRow(GlassServerControlGui_AdminList.getCount(), getLine(%data, %i));
    }

    GlassServerControlGui_AdminList.sort(1, true);
  }

  GlassServerControlGui.onWake();
}

function getServerSettingsBtn() {
  %gui = adminGui.getObject(0);

  for(%i = 0; %i < %gui.getCount(); %i++) {
    %obj = %gui.getObject(%i);

    if(%obj.command $= "AdminGui.clickServerSettings();"
    || %obj.command $= "openGlassSettings();"
    && %obj.text $= "Server Settings >>")
      return %obj;
  }

  return -1;
}

package GlassServerControlC {
  // function clientCmdsetAdminLevel(%level) {
    // if(%level > 0) {
      // GlassServerControlC.setEnabled(true);
    // } else {
      // GlassServerControlC.setEnabled(false);
    // }
    // parent::clientCmdsetAdminLevel(%level);
  // }

  // function NewPlayerListGui::update(%this, %a, %b, %c, %d, %e, %f) {
    // parent::update(%this, %a, %b, %c, %d, %e, %f);
    // GlassServerControlGui.onWake();
  // }

  // function GameConnection::setConnectArgs(%a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k, %l, %m, %n, %o,%p) {
		// return parent::setConnectArgs(%a, %b, %c, %d, %e, %f, %g, "Glass" TAB Glass.version TAB GlassClientManager.getClients() NL %h, %i, %j, %k, %l, %m, %n, %o, %p);
	// }

  function disconnect(%doReconnect) {
    GlassPrefGroup::cleanup();
    return parent::disconnect(%doReconnect);
  }

  function disconnectedCleanup(%doReconnect) {
    GlassPrefGroup::cleanup();
    return parent::disconnectedCleanup(%doReconnect);
  }

  function adminGui::onWake(%this) {
    parent::onWake(%this);

    // RTB

    if(isObject(rtbServerControlBtn)) {
      if(ServerConnection.hasGlass)
        rtbServerControlBtn.setVisible(false);
      else
        rtbServerControlBtn.setVisible(true);
    }

    // glass (takes over BL's standard server settings btn)

    %serverSettingsBtn = getServerSettingsBtn();

    if(ServerConnection.hasGlass) {
      %serverSettingsBtn.command = "openGlassSettings();";
      %serverSettingsBtn.mcolor = "50 150 250 255"; // blue
    } else {
      %serverSettingsBtn.command = "AdminGui.clickServerSettings();";
      %serverSettingsBtn.mcolor = "255 255 255 255";
    }
  }
};
activatePackage(GlassServerControlC);

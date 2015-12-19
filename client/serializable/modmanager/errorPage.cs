function GlassModManagerGui::loadErrorPage(%errorcode, %buffer) {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "0 0 0 0";
    position = "0 0";
    extent = "505 498";
  };

  %container.text = new GuiMLTextCtrl() {
    horizSizing = "center";
    vertSizing = "center";
    text = "";
    position = "102 30";
    extent = "300 498";
    autoResize = true;
  };

  %text = "<just:center><font:quicksand-bold:20><color:ff0000>Error!<br><color:000000>";
  if($GlassError[%errorcode] !$= "") {
    %text = %text @ getField($GlassError[%errorcode], 0);
    %text = %text @ "<br><br>";
    %text = %text @ "<font:quicksand-bold:16>" @ getField($GlassError[%errorcode], 1);
  } else {
    %text = %text @ "Code: " @ %errorcode;
  }

  %text = %text @ "<br><br>";

  if($Glass::Debug) {
    %text = %text @ "<just:left><font:Lucida Console:12>" @ %buffer;
  } else {
    //record the event
    %fo = new FileObject();
    %fo.openForRead("config/client/blg/error_log/" @ getrealtime() @ ".log");
    %fo.writeLine("Error Code: " @ %errorcode);
    %fo.writeLine("");
    %fo.writeLine(%buffer);
    %fo.close();
    %fo.delete();
  }

  %container.text.setValue(%text);

  %container.add(%container.text);

  GlassModManagerGui_MainDisplay.extent = %container.extent;

  GlassModManager::setLoading(false);

  GlassModManagerGui_MainDisplay.deleteAll();
  GlassModManagerGui_MainDisplay.add(%container);

  %container.text.forceReflow();

  echo(%container.text.extent);

  if(getWord(%container.text.extent, 1) > 498-30) {
    echo("long!");
    GlassModManagerGui_MainDisplay.extent = %container.extent = getWord(%container.extent, 0) SPC getWord(%container.text.extent, 1)+60;
  } else {
    GlassModManagerGui_MainDisplay.extent = 505 SPC 498;
  }


  GlassModManagerGui_MainDisplay.setVisible(true);
}


$GlassError["development"] = "Development\tThis page is still in development!";

$GlassError["status_"] = "No Status\tThe API failed to return a status message.";
$GlassError["status_error"] = "API Error\tThe API encountered an error completing your request.";
$GlassError["status_development"] = "Development\tThis page is still in development!";

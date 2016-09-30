var myname = "";
var wsUri = "ws://212.242.146.168:1978/chat";

function setupWebSocket() {
  if (document.location.href.indexOf("localhost:") >= 0) // Running from visual studio
  {
    wsUri = "ws://localhost:1978/chat";
  } else if (document.location.href.indexOf("10.0.0.6")>=0) // Running from internal server
  {
    wsUri = "ws://10.0.0.6:1978/chat";
  }
  websocket = new WebSocket(wsUri);
  websocket.onopen = function (evt) { onOpen(evt) };
  websocket.onclose = function (evt) { onClose(evt) };
  websocket.onmessage = function (evt) { onMessage(evt) };
  websocket.onerror = function (evt) { onError(evt) };
}

function onOpen(evt) {
  connected = true;
  writeToScreen("CONNECTED");
  var player = document.getElementById("intromusic");
  player.play();
  var namediv = document.getElementById("setnamediv");
  namediv.style.display = "";
  namediv.style.display = "";
  typenamediv.focus();
}
function onClose(evt) {
  writeToScreen("Disconnected. Press F5 to reconnect)");
}
function onMessage(evt) {
  processMessage(evt.data);
}
function onError(evt) {
  writeToScreen('<span style="color: red;">ERROR:</span> ' + evt.data);
}
function doSend(message) {
  if (message == null || message.length == 0)
    throw "Sending empty message";
  if (connected) {
    websocket.send(message);
  }
}

function writeToScreen(message) {
  messagesdiv.innerHTML += message + "<br />";
  messagesdiv.scrollTop = messagesdiv.scrollHeight;
}

function isEnterKey(e) {
  if (typeof e == 'undefined' && window.event) { e = window.event; }
  if (e.keyCode == 13) {
    if (myname == "") {
      var msg = typenamediv.value;
      doSend("/nick " + msg)
    } else {
      var name = typehere.value;
      doSend(name);
      typehere.value = "";
    }
  }
  return false;
}

function processMessage(json) {
  var pre = document.createElement("p");
  pre.style.wordWrap = "break-word";
  var msg = eval('(' + json + ')');
  if (msg.p) { // Tank position
      var p = msg.p.split(";");
      setTank(p[0] * 1.0, p[1] * 1.0, /*p[2] * 1.0,*/ p[2] * 1.0, p[3] * 1.0, p[4] * 1.0, p[5] * 1.0, p[6] * 1.0, p[7] * 1.0, p[8] * 1.0, p[9] * 1.0);
  } else if (msg.b) { // Bullet position
      var b = msg.b.split(";");
      if (bulletExists(b[0] * 1))
        setBullet(b[0] * 1, b[2] * 1.0, b[3] * 1.0, b[4] * 1.0, b[5] * 1.0, b[6] * 1.0, b[7] * 1.0);
      else
      {
        var bullet = new Bullet(b[0] * 1, b[1] * 1.0, b[2] * 1.0, b[3] * 1.0, b[4] * 1.0, b[5] * 1.0, b[6] * 1.0, b[7] * 1.0);
        bullets.push(bullet);
      }
  } else {
    switch (msg.type) {
      case "join":
        writeToScreen("<span style=\"color: #009900;\">* " + msg.name + " connected</span>");
        tanks[msg.id] = new Tank(msg.id, msg.name);
        updateClientList();
        break;
      case "quit":
        writeToScreen("<span style=\"color: #000099;\">* " + tanks[msg.id].name + " disconnected</span>");
        removeTank(msg.id);
        updateClientList();
        break;
      case "chat":
        if (msg.id == -1)
          writeToScreen("<span style=\"color: #009999;\">* " + msg.text + "</span>");
        else if (msg.id == myid)
          writeToScreen("<span style=\"color: #009900;\">&lt;" + tanks[msg.id].name + "&gt;</span>&nbsp;" + msg.text);
        else
          writeToScreen("&lt;" + tanks[msg.id].name + "&gt; " + msg.text);
        break;
      case "nick":
        if (msg.id == myid) {
          writeToScreen("<span style=\"color: #009900;\">* you are now know as " + msg.name + "</span>");
          myname = msg.name;
        } else {
          writeToScreen("<span style=\"color: #009900;\">* " + msg.oldname + " changed name to " + msg.newname + "</span>");
          for (var i in tanks) {
            if (tanks[i].id == msg.id) {
              tanks[i].name = msg.name;
              break;
            }
          }
        }
        updateClientList();
        break;
      case "id":
        if (msg.name != null) {
          myid = msg.id;
          tanks[msg.id] = new Tank(myid, msg.name);
          myname = msg.name;
          updateClientList();
          var namediv = document.getElementById("setnamediv");
          namediv.style.display = "none";
          var backdiv = document.getElementById("backdiv");
          backdiv.style.display = "none";
          document.onkeydown = handleKeyDown;
          document.onkeyup = handleKeyUp;
          var player = document.getElementById("intromusic");
          player.pause();
          //writeToScreen("Music paused");
        }
        break;
      case "error":
        if (myname == "")
          namemsg.innerHTML = msg.text;
        else
          writeToScreen("<span style=\"color: #990000;\">* ERROR: " + msg.text + "</span>");
        break;
      case "clients":
        for (var i = 0; i < msg.list.length; i++) {
          if (myid != msg.list[i].id)
            tanks[msg.list[i].id] = new Tank(msg.list[i].id, msg.list[i].name);
        }
        updateClientList();
        break;
      case "landscape":
        addLandscape(msg.data);
        writeToScreen("Landscape data received");
        setInterval("tick()", 10);
        animate();
        break;
      case "bhit":
        writeToScreen("Bullet hit ground");
        var obj = null;
        for (var i= bullets.length - 1; i >= 0; i--) {
          if (bullets[i]) {
            if (bullets[i].id == msg.id) {
              obj = bullets[i].obj;
              explosions.push(new Explotion(obj.position.x, obj.position.y, obj.position.z, 50, 10, 5));
              bullets.splice(i, 1);
              break;
            }
          }
        }
        if (obj != null)
          scene.remove(obj);
        var bskidx = Math.round(toSkidmarksX(msg.x));
        var bskidy = Math.round(toSkidmarksZ(msg.z));
        ctx.fillStyle = "rgba(0, 0, 0, 0.25)";
        ctx.beginPath();
        ctx.arc(bskidx, bskidy, 1, 0, Math.PI * 2, true);
        ctx.fill();
        ctx.arc(bskidx, bskidy, 2, 0, Math.PI * 2, true);
        ctx.fill();
        ctx.arc(bskidx, bskidy, 3, 0, Math.PI * 2, true);
        ctx.fill();
        ctx.arc(bskidx, bskidy, 4, 0, Math.PI * 2, true);
        ctx.fill();
        ctx.closePath();
        texture1.needsUpdate = true;
        break;
      case "vhit":
        writeToScreen("Bullet hit vehicle");
        var obj = null;
        for (var i = bullets.length - 1; i >= 0; i++) {
          if (bullets[i]) {
            if (bullets[i].id == msg.bid) {
              obj = bullets[i].obj;
              explosions.push(new Explotion(obj.position.x, obj.position.y, obj.position.z, 100, 20, 10));
              bullets.splice(i, 1);
              break;
            }
          }
        }
        if (obj != null)
          scene.remove(obj);
        var vskidx = Math.round(toSkidmarksX(msg.x));
        var vskidy = Math.round(toSkidmarksZ(msg.z));
        ctx.fillStyle = "rgba(0, 0, 0, 0.25)";
        ctx.beginPath();
        ctx.arc(vskidx, vskidy, 1, 0, Math.PI * 2, true);
        ctx.fill();
        ctx.arc(vskidx, vskidy, 2, 0, Math.PI * 2, true);
        ctx.fill();
        ctx.arc(vskidx, vskidy, 3, 0, Math.PI * 2, true);
        ctx.fill();
        ctx.arc(vskidx, vskidy, 4, 0, Math.PI * 2, true);
        ctx.fill();
        ctx.closePath();
        texture1.needsUpdate = true;
        if (msg.vid == myid)
          writeToScreen("You were hit");
        else {
          tanks[msg.vid].tankbody.visible = false;
          THREE.SceneUtils.traverseHierarchy(tanks[msg.vid].tankbody, function (object) { object.visible = false; });
          writeToScreen(tanks[msg.vid].name + " were hit");
        }
        break;
    }
  }
}

function updateClientList() {
  var html = "";
  for (var i in tanks) {
    if (tanks[i] != null)
      html += tanks[i].name + "<br />";
  }
  clientlistdiv.innerHTML = html;
}

function removeTank(id) {
  id = id * 1;
  for (var i in tanks) {
    if (tanks[i] != null && (tanks[i].id*1) == id)
    {
      scene.remove(tanks[i].tankbody);
      delete tanks[i];
      break;
    }
  }
}
/*
This file is part of Armored Deathmatch by Hans Milling.

Armored Deathmatch is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Armored Deathmatch is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Armored Deathmatch.  If not, see <http://www.gnu.org/licenses/>.
	
*/

var myname = "";
var wsUri = null;
var fullscreenRequested = false;

function setupWebSocket() {
    // Build WebSocket URI relative to the current page so it's not bound to a specific domain or IP.
    // Use wss when the page is served over https, otherwise use ws. If the page has no host (file://),
    // fall back to localhost:8005 for development.
    var host = window.location.host;
    var scheme = (window.location.protocol === 'https:') ? 'wss:' : 'ws:';
    wsUri = scheme + '//' + host + '/chat';
    websocket = new WebSocket(wsUri);
    websocket.onopen = function (evt) { onOpen(evt) };
    websocket.onclose = function (evt) { onClose(evt) };
    websocket.onmessage = function (evt) { onMessage(evt) };
    websocket.onerror = function (evt) { onError(evt) };
}

function fadeOutAudio(audio, durationMs) {
    if (!audio || typeof audio.volume !== 'number') {
        return;
    }
    var startVolume = audio.volume;
    var steps = Math.max(1, Math.floor(durationMs / 50));
    var step = 0;
    var fadeInterval = setInterval(function () {
        step++;
        var progress = step / steps;
        audio.volume = Math.max(0, startVolume * (1 - progress));
        if (progress >= 1) {
            clearInterval(fadeInterval);
            audio.pause();
            audio.volume = startVolume;
        }
    }, 50);
}

function onOpen(evt) {
    connected = true;
    writeToScreen("CONNECTED");
    var player = document.getElementById("intromusic");
    player.play().catch(() => {
        document.addEventListener('click', function _once() { player.play().catch(() => { }); document.removeEventListener('click', _once); }, { once: true });
    });
    var namediv = document.getElementById("setnamediv");
    namediv.style.display = "";
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
            requestFullscreen();
            doSend("/nick " + msg)
        } else {
            var name = typehere.value;
            doSend(name);
            typehere.value = "";
        }
    }
    return false;
}

function requestFullscreen() {
    if (fullscreenRequested)
        return;
    var element = document.documentElement;
    if (!element)
        return;
    var request = element.requestFullscreen
        || element.webkitRequestFullscreen
        || element.mozRequestFullScreen
        || element.msRequestFullscreen
        || element.webkitRequestFullScreen;
    if (!request)
        return;
    fullscreenRequested = true;
    try {
        var result = request.call(element);
        if (result && typeof result.catch === 'function') {
            result.catch(function () { });
        }
    } catch (err) {
        fullscreenRequested = false;
    }
    tryRequestPointerLock();
}

function tryRequestPointerLock() {
    try {
        if (typeof requestPointerLockForCanvas === 'function') {
            requestPointerLockForCanvas();
            return;
        }
        if (typeof pointerLockElement !== 'undefined' && pointerLockElement) {
            var request = pointerLockElement.requestPointerLock
                || pointerLockElement.mozRequestPointerLock
                || pointerLockElement.webkitRequestPointerLock;
            if (request) {
                var lockPromise = request.call(pointerLockElement);
                if (lockPromise && typeof lockPromise.catch === 'function') {
                    lockPromise.catch(function () { });
                }
            }
        }
    } catch (err) {
        // ignore pointer lock failures; user can retry with ESC/click
    }
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
        var bulletId = b[0] * 1;
        var shooterId = b[1] * 1;
        var spawnX = b[2] * 1.0;
        var spawnY = b[3] * 1.0;
        var spawnZ = b[4] * 1.0;
        var velX = b[5] * 1.0;
        var velY = b[6] * 1.0;
        var velZ = b[7] * 1.0;
        if (bulletExists(bulletId))
            setBullet(bulletId, spawnX, spawnY, spawnZ, velX, velY, velZ);
        else {
            var bullet = new Bullet(bulletId, shooterId * 1.0, spawnX, spawnY, spawnZ, velX, velY, velZ);
            bullets.push(bullet);
            if (myid >= 0 && shooterId === myid) {
                playLocalShotSound();
            } else {
                playDistantShotSoundAt(spawnX, spawnY, spawnZ);
            }
        }
    } else {
        switch (msg.type) {
            case "join":
                writeToScreen("<span style=\"color: #009900;\">* " + msg.name + " connected</span>");
                if (!tanks[msg.id]) tanks[msg.id] = new Tank(msg.id, msg.name);
                tanks[msg.id].name = msg.name;
                tanks[msg.id].score = msg.score || 0;
                updateClientList();
                break;
            case "respawn":
                writeToScreen("<span style=\"color: #009900;\">* " + msg.name + " respawned</span>");
                tanks[msg.id].tankbody.visible = true;
                if (tanks[msg.id].tankbody.traverse) {
                    tanks[msg.id].tankbody.traverse(function (object) { object.visible = true; });
                }
                if (msg.id == myid) {
                    window.setCameraFollow();
                    window.positionCameraBehindPlayer();
                }
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
                            tanks[i].score = (typeof msg.score !== 'undefined') ? msg.score : (tanks[i].score || 0);
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
                    tanks[msg.id].score = msg.score || 0;
                    myname = msg.name;
                    updateClientList();
                    var namediv = document.getElementById("setnamediv");
                    namediv.style.display = "none";
                    var backdiv = document.getElementById("backdiv");
                    backdiv.style.display = "none";
                    document.onkeydown = handleKeyDown;
                    document.onkeyup = handleKeyUp;
                    var player = document.getElementById("intromusic");
                    fadeOutAudio(player, 5000);
                    //writeToScreen("Music paused");
                    window.setCameraFollow();
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
                    var entry = msg.list[i];
                    var id = entry.id;
                    if (!tanks[id]) {
                        tanks[id] = new Tank(id, entry.name);
                    }
                    tanks[id].name = entry.name;
                    tanks[id].score = (typeof entry.score !== 'undefined') ? entry.score : (tanks[id].score || 0);
                }
                updateClientList();
                break;
            case "landscape":
                addLandscape(msg.data);
                writeToScreen("Landscape data received");
                // Use a function reference and slightly lower frequency to avoid starving the main thread
                setInterval(tick, 30);
                animate();
                break;
            case "bhit":
                //writeToScreen("Bullet hit ground");
                var obj = null;
                for (var i = bullets.length - 1; i >= 0; i--) {
                    if (bullets[i]) {
                        if (bullets[i].id == msg.id) {
                            obj = bullets[i].obj;
                            explosions.push(new Explosion(obj.position.x, obj.position.y, obj.position.z, 50, 10, 5));
                            bullets.splice(i, 1);
                            break;
                        }
                    }
                }
                if (obj != null) {
                    try { scene.remove(obj); } catch (e) { }
                }
                var soundPos = null;
                if (obj && obj.position) {
                    soundPos = { x: obj.position.x, y: obj.position.y, z: obj.position.z };
                }
                // Draw skidmarks only if coordinates present and numeric
                var bx = (typeof msg.x !== 'undefined') ? Number(msg.x) : NaN;
                var bz = (typeof msg.z !== 'undefined') ? Number(msg.z) : NaN;
                if (isFinite(bx) && isFinite(bz)) {
                    var bskidx = Math.round(toSkidmarksX(bx));
                    var bskidy = Math.round(toSkidmarksZ(bz));
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
                    if (texture1) {
                        texture1.needsUpdate = true;
                    }
                    if (!soundPos) {
                        var groundY = 0;
                        try {
                            groundY = pointHeight(toHeightMapX(bx), toHeightMapZ(bz));
                        } catch (e) {
                            groundY = 0;
                        }
                        soundPos = { x: bx, y: groundY, z: bz };
                    }
                }
                if (soundPos) {
                    playHitGroundSoundAt(soundPos.x, soundPos.y, soundPos.z);
                }
                break;
            case "vhit":
                //writeToScreen("Bullet hit vehicle");
                var obj = null;
                var vehicleSoundPos = null;
                for (var i = bullets.length - 1; i >= 0; i--) {
                    if (bullets[i]) {
                        if (bullets[i].id == msg.bid) {
                            obj = bullets[i].obj;
                            explosions.push(new Explosion(obj.position.x, obj.position.y, obj.position.z, 100, 20, 10));
                            vehicleSoundPos = { x: obj.position.x, y: obj.position.y, z: obj.position.z };
                            bullets.splice(i, 1);
                            break;
                        }
                    }
                }
                if (obj != null)
                    scene.remove(obj);
                // Draw skidmarks only if coordinates present and numeric
                var xnum = (typeof msg.x !== 'undefined') ? Number(msg.x) : NaN;
                var znum = (typeof msg.z !== 'undefined') ? Number(msg.z) : NaN;
                if (isFinite(xnum) && isFinite(znum)) {
                    var vskidx = Math.round(toSkidmarksX(xnum));
                    var vskidy = Math.round(toSkidmarksZ(znum));
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
                    if (!vehicleSoundPos) {
                        var vhGroundY = 0;
                        try {
                            vhGroundY = pointHeight(toHeightMapX(xnum), toHeightMapZ(znum));
                        } catch (e) {
                            vhGroundY = 0;
                        }
                        vehicleSoundPos = { x: xnum, y: vhGroundY, z: znum };
                    }
                }
                // If vid refers to a tank we know about, hide its body; otherwise skip to avoid errors
                if (msg.vid != null && tanks[msg.vid] && tanks[msg.vid].tankbody) {
                    if (msg.vid == myid)
                        writeToScreen("You were hit");
                    else {
                        tanks[msg.vid].tankbody.visible = false;
                        if (tanks[msg.vid].tankbody.traverse) {
                            tanks[msg.vid].tankbody.traverse(function (object) { object.visible = false; });
                        }
                        writeToScreen(tanks[msg.vid].name + " were hit");
                    }
                } else {
                    // vid unknown: just log hit
                    if (msg.vid == myid)
                        writeToScreen("You were hit");
                    else
                        writeToScreen("A vehicle was hit");
                }
                if (msg.vid == myid) {
                    window.setCameraOverview();
                }
                if (vehicleSoundPos && msg.vid != myid) {
                    playHitVehicleSoundAt(vehicleSoundPos.x, vehicleSoundPos.y, vehicleSoundPos.z);
                }
                break;
        }
    }
}

function updateClientList() {
    // Render client list with names left-aligned and scores right-aligned
    var html = "<table style='width:100%; border-collapse: collapse;'>";
    for (var i in tanks) {
        if (tanks[i] != null) {
            var score = (tanks[i].score !== undefined) ? tanks[i].score : 0;
            html += "<tr><td style='text-align:left; padding-right:10px;'>" + tanks[i].name + "</td><td style='text-align:right; width:60px;'>" + score + "</td></tr>";
        }
    }
    html += "</table>";
    clientlistdiv.innerHTML = html;
}

function removeTank(id) {
    id = id * 1;
    for (var i in tanks) {
        if (tanks[i] != null && (tanks[i].id * 1) == id) {
            scene.remove(tanks[i].tankbody);
            delete tanks[i];
            break;
        }
    }
}
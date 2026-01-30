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

var currentlyPressedKeys = {};

var KeyPrimaryLeft = 65; // A Key
var KeyPrimaryRight = 68; // D Key
var KeyPrimaryUp = 87; // W Key
var KeyPrimaryDown = 83; // S key
var KeySecondaryLeft = 37; //Cursor Left
var KeySecondaryRight = 39; // Cursor Right
var KeySecondaryUp = 38; // Cursor Up
var KeySecondaryDown = 40; // Cursor Down
var KeyFire = 32; // Space

function handleKeyDown(event) {
  if (!currentlyPressedKeys[event.keyCode]) {
    switch (event.keyCode) {
      case KeyPrimaryLeft:
        doSend("+1");
        break;
      case KeyPrimaryRight:
        doSend("+2");
        break;
      case KeyPrimaryUp:
        doSend("+3");
        break;
      case KeyPrimaryDown:
        doSend("+4");
        break;
      case KeySecondaryLeft:
        doSend("+5");
        break;
      case KeySecondaryRight:
        doSend("+6");
        break;
      case KeySecondaryUp:
        doSend("+7");
        break;
      case KeySecondaryDown:
        doSend("+8");
        break;
      case KeyFire:
        var bstart = new THREE.Vector3().setFromMatrixPosition(tanks[myid].tankbarrel.matrixWorld);
        var vector = new THREE.Vector3(0, 0, 56);
        var bend = vector.applyMatrix4(tanks[myid].tankbarrel.matrixWorld);
        var direction = bstart.subVectors(bend, bstart);
        direction.normalize();
        doSend("+9;" + bend.x + ";" + bend.y + ";" + bend.z + ";" + direction.x + ";" + direction.y + ";" + direction.z);
        break;
    }
  }
  currentlyPressedKeys[event.keyCode] = true;
}

function handleKeyUp(event) {
  switch (event.keyCode) {
    case KeyPrimaryLeft:
      doSend("-1");
      break;
    case KeyPrimaryRight:
      doSend("-2");
      break;
    case KeyPrimaryUp:
      doSend("-3");
      break;
    case KeyPrimaryDown:
      doSend("-4");
      break;
    case KeySecondaryLeft:
      doSend("-5");
      break;
    case KeySecondaryRight:
      doSend("-6");
      break;
    case KeySecondaryUp:
      doSend("-7");
      break;
    case KeySecondaryDown:
      doSend("-8");
      break;
    case KeyFire:
      doSend("-9");
      break;
  }
  currentlyPressedKeys[event.keyCode] = false;
}

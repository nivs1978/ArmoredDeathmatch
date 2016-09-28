var currentlyPressedKeys = {};

var KeyPrimaryLeft = 37; //Cursor Left
var KeyPrimaryRight = 39; // Cursor Right
var KeyPrimaryUp = 38; // Cursor Up
var KeyPrimaryDown = 40; // Cursor Down
var KeySecondaryLeft = 65; // A Key
var KeySecondaryRight = 68; // D Key
var KeySecondaryUp = 87; // W Key
var KeySecondaryDown = 83; // S key
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
        var barrelmatrix = new THREE.Matrix4().multiplyMatrices(tanks[myid].tankbarrel.matrixWorld, tanks[myid].tankbarrel.matrix);
        var vector = new THREE.Vector3(0, 0, 56);
        var shellpos = vector.applyMatrix4( tanks[myid].tankbarrel.matrixWorld);
        var bstart = new THREE.Vector3(tanks[myid].tankbarrel.matrixWorld.n14, tanks[myid].tankbarrel.matrixWorld.n24, tanks[myid].tankbarrel.matrixWorld.n34);
        var bend = new THREE.Vector3(shellpos.x, shellpos.y, shellpos.z);
        var direction = bstart.subVectors(bend, bstart);
        direction.normalize();
        doSend("+9;" + shellpos.x + ";" + shellpos.y + ";" + shellpos.z + ";" + direction.x + ";" + direction.y + ";" + direction.z);
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

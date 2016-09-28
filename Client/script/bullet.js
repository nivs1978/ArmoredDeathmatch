var bullets = [];

function Bullet(i, vi, x, y, z, vx, vy, vz) {
  this.id = i;
  this.vehicleid = vi;
  this.velocity = {
    "x": vx,
    "y": vy,
    "z": vz
  };

  /*
  var mats = [];
  mats.push(new THREE.MeshBasicMaterial({ color: 0x009e60 }));
  mats.push(new THREE.MeshBasicMaterial({ color: 0x009e60 }));
  mats.push(new THREE.MeshBasicMaterial({ color: 0x0051ba }));
  mats.push(new THREE.MeshBasicMaterial({ color: 0x0051ba }));
  mats.push(new THREE.MeshBasicMaterial({ color: 0xffd500 }));
  mats.push(new THREE.MeshBasicMaterial({ color: 0xffd500 }));
  mats.push(new THREE.MeshBasicMaterial({ color: 0xff5800 }));
  mats.push(new THREE.MeshBasicMaterial({ color: 0xff5800 }));
  mats.push(new THREE.MeshBasicMaterial({ color: 0xC41E3A }));
  mats.push(new THREE.MeshBasicMaterial({ color: 0xC41E3A }));
  mats.push(new THREE.MeshBasicMaterial({ color: 0xffffff }));
  mats.push(new THREE.MeshBasicMaterial({ color: 0xffffff }));
  var faceMaterial = new THREE.MeshFaceMaterial(mats);

  var cubeGeom = new THREE.BoxGeometry(2.9, 2.9, 2.9);
  var cube = new THREE.Mesh(cubeGeom, faceMaterial);*/
  var geom = new THREE.BoxGeometry(10, 10, 10, 1, 1, 1);
  var material = new THREE.MeshLambertMaterial({ color: 0xFFFFFF, ambient: 0x808080 });
  this.obj = new THREE.Mesh(geom, material); // new THREE.MeshFaceMaterial([material])
  this.obj.position.x = x;
  this.obj.position.y = y;
  this.obj.position.z = z;
  scene.add(this.obj);
}

function bulletExists(id) {
  for (var idx in bullets)
    if (bullets[idx].id == id)
      return true;
  return false;
}

function setBullet(id, x, y, z, vx, vy, vz) {
  for (var idx in bullets) {
    if (bullets[idx].id == id) {
      bullets[idx].obj.position.x = x;
      bullets[idx].obj.position.y = y;
      bullets[idx].obj.position.z = z;
      bullets[idx].velocity.x = vx;
      bullets[idx].velocity.y = vy;
      bullets[idx].velocity.z = vz;
      break;
    }
  }
}
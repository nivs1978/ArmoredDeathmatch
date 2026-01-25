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

var bullets = [];

function Bullet(i, vi, x, y, z, vx, vy, vz) {
  this.id = i;
  this.vehicleid = vi;
  this.velocity = {
    "x": vx,
    "y": vy,
    "z": vz
  };

  var geom = new THREE.BoxGeometry(10, 10, 10, 1, 1, 1);
  var material = new THREE.MeshLambertMaterial({ color: 0xFFFFFF, ambient: 0x808080 });
  this.obj = new THREE.Mesh(geom, material);
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
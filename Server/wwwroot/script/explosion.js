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

var explosions = [];

function Explosion(x, y, z, particles, size, velocity) {
  this.x = x;
  this.y = y;
  this.z = z; // Get landscape height
  this.particles = particles;
  this.size = size;
  this.velocity = velocity;
  this.frame = 200;
  this.cubes = [];
    var material = new THREE.MeshLambertMaterial({ color: 0xFFFF00, emissive: 0x000000 });
  for (var i = 0; i < this.particles; i++) {
    var obj = new THREE.Mesh(new THREE.BoxGeometry(this.size, this.size, this.size, 1, 1, 1), material);
    obj.position.x = this.x;
    obj.position.y = this.y;
    obj.position.z = this.z;
    var dv = velocity * 2;
    obj.velocity = { // Set random direction
      "x": Math.random()*dv-velocity,
      "y": velocity + Math.random()*velocity,
      "z": Math.random()*dv-velocity
    };
    scene.add(obj);
    this.cubes.push(obj);

    this.tick = function () {
      for (var i = 0; i < this.particles; i++) {
        var cube = this.cubes[i];
        cube.position.x += cube.velocity.x;
        cube.position.y += cube.velocity.y;
        cube.position.z += cube.velocity.z;
        cube.velocity.y -= 0.25;
      }
      this.frame--;
    }

  }
}

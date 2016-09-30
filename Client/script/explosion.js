var explosions = [];

function Explosion(x, y, z, particles, size, velocity) {
  this.x = x;
  this.y = y;
  this.z = z; // Get landscape height
  this.particles = particles;
  this.size = size;
  this.velocity = velocity;
  this.frame = 100;
  this.cubes = [];
  var material = new THREE.MeshLambertMaterial({ color: 0xFFFF00, ambient: 0xffffff });
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
        cube.velocity.y -= 0.5;
      }
      this.frame--;
    }

  }
}

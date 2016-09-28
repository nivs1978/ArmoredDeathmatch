var tanks = [];
function Tank(i, n) {
  this.id = i;
  this.name = n;
  this.dead = false;
  this.vel = [0, 0, 0];
  this.velocity = 0.0;
  this.maxvelocity = 3.0;
  this.acceleration = 0.05;
  this.deceleration = 0.1;
  this.turnspeed = 0.0;
  this.turnacceleration = 0.001;
  this.turndeceleration = 0.003;
  this.maxturnspeed = 0.01;
  this.turret = {
    "turnspeed": 0.0,
    "turnacceleration": 0.001,
    "turndeceleration": 0.003,
    "maxturnspeed": 0.01
  };
  this.barrel = {
    "turnspeed": 0.0,
    "turnacceleration": 0.001,
    "turndeceleration": 0.003,
    "maxturnspeed": 0.005,
    "minturn": 0.0,
    "maxturn": 0.3
  };

  var tankbodyleft = new THREE.MeshLambertMaterial({ color: 0xffffff, map: THREE.ImageUtils.loadTexture('textures/m1_body_left.png'), ambient: 0x333333 });
  var tankbodyrigh = new THREE.MeshLambertMaterial({ color: 0xffffff, map: THREE.ImageUtils.loadTexture('textures/m1_body_right.png'), ambient: 0x333333 });
  var tankbodytop = new THREE.MeshLambertMaterial({ color: 0xffffff, map: THREE.ImageUtils.loadTexture('textures/m1_body_top.png'), ambient: 0x3333333 });
  var tankbodybottom = new THREE.MeshLambertMaterial({ color: 0xffffff, map: THREE.ImageUtils.loadTexture('textures/m1_body_bottom.png'), ambient: 0x333333 });
  var tankbodyfront = new THREE.MeshLambertMaterial({ color: 0xffffff, map: THREE.ImageUtils.loadTexture('textures/m1_body_front.png'), ambient: 0x333333 });
  var tankbodyback = new THREE.MeshLambertMaterial({ color: 0xffffff, map: THREE.ImageUtils.loadTexture('textures/m1_body_back.png'), ambient: 0x333333 });
  var bodymaterials = [
    tankbodyleft,   // Left side
    tankbodyrigh,   // Right side
    tankbodytop,    // Top side
    tankbodybottom, // Bottom side
    tankbodyfront,  // Front side
    tankbodyback    // Back side
  ];

  this.tankbody = new THREE.Mesh(new THREE.BoxGeometry(50, 50, 50, 1, 2, 1), new THREE.MeshFaceMaterial(bodymaterials));

  this.tankbody.geometry.vertices[0].x = 40;
  this.tankbody.geometry.vertices[0].y = 10;
  this.tankbody.geometry.vertices[0].z = 50;

  this.tankbody.geometry.vertices[1].x = 40;
  this.tankbody.geometry.vertices[1].y = 10;
  this.tankbody.geometry.vertices[1].z = -50;

  this.tankbody.geometry.vertices[2].x = 40;
  this.tankbody.geometry.vertices[2].y = 0;
  this.tankbody.geometry.vertices[2].z = 70;

  this.tankbody.geometry.vertices[3].x = 40;
  this.tankbody.geometry.vertices[3].y = 0;
  this.tankbody.geometry.vertices[3].z = -70;

  this.tankbody.geometry.vertices[4].x = 40;
  this.tankbody.geometry.vertices[4].y = -20;
  this.tankbody.geometry.vertices[4].z = 50;

  this.tankbody.geometry.vertices[5].x = 40;
  this.tankbody.geometry.vertices[5].y = -20;
  this.tankbody.geometry.vertices[5].z = -50;

  this.tankbody.geometry.vertices[6].x = -40;
  this.tankbody.geometry.vertices[6].y = 10;
  this.tankbody.geometry.vertices[6].z = -50;

  this.tankbody.geometry.vertices[7].x = -40;
  this.tankbody.geometry.vertices[7].y = 10;
  this.tankbody.geometry.vertices[7].z = 50;

  this.tankbody.geometry.vertices[8].x = -40;
  this.tankbody.geometry.vertices[8].y = 0;
  this.tankbody.geometry.vertices[8].z = -70;

  this.tankbody.geometry.vertices[9].x = -40;
  this.tankbody.geometry.vertices[9].y = 0;
  this.tankbody.geometry.vertices[9].z = 70;

  this.tankbody.geometry.vertices[10].x = -40;
  this.tankbody.geometry.vertices[10].y = -20;
  this.tankbody.geometry.vertices[10].z = -50;

  this.tankbody.geometry.vertices[11].x = -40;
  this.tankbody.geometry.vertices[11].y = -20;
  this.tankbody.geometry.vertices[11].z = 50;

  this.tankbody.position.x += 0;
  this.tankbody.position.z += 0;

  // Adjust textures:
  uvs = this.tankbody.geometry.faceVertexUvs[0];

  uvs[0][0].u = 0.1;
  uvs[0][0].v = 0.0;
  uvs[0][1].u = 0.0;
  uvs[0][1].v = 0.5;
  uvs[0][2].u = 1.0;
  uvs[0][2].v = 0.5;
//  uvs[0][3].u = 0.8;
//  uvs[0][3].v = 0.0;

  uvs[1][0].u = 0.0;
  uvs[1][0].v = 0.5;
  uvs[1][1].u = 0.2;
  uvs[1][1].v = 1.0;
  uvs[1][2].u = 0.9;
  uvs[1][2].v = 1.0;
//  uvs[1][3].u = 1.0;
//  uvs[1][3].v = 0.5;

  uvs[2][0].u = 0.1;
  uvs[2][0].v = 0.0;
  uvs[2][1].u = 0.0;
  uvs[2][1].v = 0.5;
  uvs[2][2].u = 1.0;
  uvs[2][2].v = 0.5;
//  uvs[2][3].u = 0.8;
//  uvs[2][3].v = 0.0;

  uvs[3][0].u = 0.0;
  uvs[3][0].v = 0.5;
  uvs[3][1].u = 0.2;
  uvs[3][1].v = 1.0;
  uvs[3][2].u = 0.9;
  uvs[3][2].v = 1.0;
//  uvs[3][3].u = 1.0;
//  uvs[3][3].v = 0.5;    
  this.tankbody.geometry.__dirtyUvs = true;
  this.tankbody.geometry.dynamic = true;
  
  // Tank turret;

  var tankturretleft = new THREE.MeshLambertMaterial({ color: 0xffffff, map: THREE.ImageUtils.loadTexture('textures/m1_turret_left.png'), ambient: 0x333333 });
  var tankturretrigh = new THREE.MeshLambertMaterial({ color: 0xffffff, map: THREE.ImageUtils.loadTexture('textures/m1_turret_right.png'), ambient: 0x333333 });
  var tankturrettop = new THREE.MeshLambertMaterial({ color: 0xffffff, map: THREE.ImageUtils.loadTexture('textures/m1_turret_top.png'), ambient: 0x3333333 });
  var tankturretbottom = new THREE.MeshLambertMaterial({ color: 0xffffff, map: THREE.ImageUtils.loadTexture('textures/m1_turret_bottom.png'), ambient: 0x333333 });
  var tankturretfront = new THREE.MeshLambertMaterial({ color: 0xffffff, map: THREE.ImageUtils.loadTexture('textures/m1_turret_front.png'), ambient: 0x333333 });
  var tankturretback = new THREE.MeshLambertMaterial({ color: 0xffffff, map: THREE.ImageUtils.loadTexture('textures/m1_turret_back.png'), ambient: 0x333333 });
  var turretmaterials = [
    tankturretleft,   // Left side
    tankturretrigh,   // Right side
    tankturrettop,    // Top side
    tankturretbottom, // Bottom side
    tankturretfront,  // Front side
    tankturretback    // Back side
  ];

  this.tankturret = new THREE.Mesh(new THREE.BoxGeometry(50, 50, 50, 1, 2, 1), new THREE.MeshFaceMaterial(turretmaterials));

  this.tankturret.geometry.vertices[0].x = 20;
  this.tankturret.geometry.vertices[0].y = 30;
  this.tankturret.geometry.vertices[0].z = 10;

  this.tankturret.geometry.vertices[1].x = 20;
  this.tankturret.geometry.vertices[1].y = 30;
  this.tankturret.geometry.vertices[1].z = -30;

  this.tankturret.geometry.vertices[2].x = 30;
  this.tankturret.geometry.vertices[2].y = 20;
  this.tankturret.geometry.vertices[2].z = 30;

  this.tankturret.geometry.vertices[3].x = 30;
  this.tankturret.geometry.vertices[3].y = 20;
  this.tankturret.geometry.vertices[3].z = -40;

  this.tankturret.geometry.vertices[4].x = 30;
  this.tankturret.geometry.vertices[4].y = 10;
  this.tankturret.geometry.vertices[4].z = 20;

  this.tankturret.geometry.vertices[5].x = 30;
  this.tankturret.geometry.vertices[5].y = 10;
  this.tankturret.geometry.vertices[5].z = -30;

  this.tankturret.geometry.vertices[6].x = -20;
  this.tankturret.geometry.vertices[6].y = 30;
  this.tankturret.geometry.vertices[6].z = -30;

  this.tankturret.geometry.vertices[7].x = -20;
  this.tankturret.geometry.vertices[7].y = 30;
  this.tankturret.geometry.vertices[7].z = 10;

  this.tankturret.geometry.vertices[8].x = -30;
  this.tankturret.geometry.vertices[8].y = 20;
  this.tankturret.geometry.vertices[8].z = -40;

  this.tankturret.geometry.vertices[9].x = -30;
  this.tankturret.geometry.vertices[9].y = 20;
  this.tankturret.geometry.vertices[9].z = 30;

  this.tankturret.geometry.vertices[10].x = -30;
  this.tankturret.geometry.vertices[10].y = 10;
  this.tankturret.geometry.vertices[10].z = -30;

  this.tankturret.geometry.vertices[11].x = -30;
  this.tankturret.geometry.vertices[11].y = 10;
  this.tankturret.geometry.vertices[11].z = 20;

  this.tankturret.rotation.y = 0;// -2 * Math.PI / 4;

  this.tankturret.geometry.__dirtyUvs = true;
  this.tankturret.geometry.dynamic = true;

  // Tank barrel

  var tankbarrelleft = new THREE.MeshLambertMaterial({ color: 0xffffff, map: THREE.ImageUtils.loadTexture('textures/m1_barrel_left.png'), ambient: 0x333333 });
  var tankbarrelrigh = new THREE.MeshLambertMaterial({ color: 0xffffff, map: THREE.ImageUtils.loadTexture('textures/m1_barrel_right.png'), ambient: 0x333333 });
  var tankbarreltop = new THREE.MeshLambertMaterial({ color: 0xffffff, map: THREE.ImageUtils.loadTexture('textures/m1_barrel_top.png'), ambient: 0x3333333 });
  var tankbarrelbottom = new THREE.MeshLambertMaterial({ color: 0xffffff, map: THREE.ImageUtils.loadTexture('textures/m1_barrel_bottom.png'), ambient: 0x333333 });
  var tankbarrelfront = new THREE.MeshLambertMaterial({ color: 0xffffff, map: THREE.ImageUtils.loadTexture('textures/m1_barrel_front.png'), ambient: 0x333333 });
  var tankbarrelback = new THREE.MeshLambertMaterial({ color: 0xffffff, map: THREE.ImageUtils.loadTexture('textures/m1_barrel_back.png'), ambient: 0x333333 });
  var barrelmaterials = [
    tankbarrelleft,   // Left side
    tankbarrelrigh,   // Right side
    tankbarreltop,    // Top side
    tankbarrelbottom, // Bottom side
    tankbarrelfront,  // Front side
    tankbarrelback    // Back side
  ];

  this.tankbarrel = new THREE.Mesh(new THREE.BoxGeometry(50, 50, 50, 1, 1, 1), new THREE.MeshFaceMaterial(barrelmaterials));

  this.tankbarrel.geometry.vertices[0].x = 3;
  this.tankbarrel.geometry.vertices[0].y = 3;
  this.tankbarrel.geometry.vertices[0].z = 56;

  this.tankbarrel.geometry.vertices[1].x = 3;
  this.tankbarrel.geometry.vertices[1].y = 3;
  this.tankbarrel.geometry.vertices[1].z = 0;

  this.tankbarrel.geometry.vertices[2].x = 3;
  this.tankbarrel.geometry.vertices[2].y = -3;
  this.tankbarrel.geometry.vertices[2].z = 56;

  this.tankbarrel.geometry.vertices[3].x = 3;
  this.tankbarrel.geometry.vertices[3].y = -3;
  this.tankbarrel.geometry.vertices[3].z = 0;

  this.tankbarrel.geometry.vertices[4].x = -3;
  this.tankbarrel.geometry.vertices[4].y = 3;
  this.tankbarrel.geometry.vertices[4].z = 0;

  this.tankbarrel.geometry.vertices[5].x = -3;
  this.tankbarrel.geometry.vertices[5].y = 3;
  this.tankbarrel.geometry.vertices[5].z = 56;

  this.tankbarrel.geometry.vertices[6].x = -3;
  this.tankbarrel.geometry.vertices[6].y = -3;
  this.tankbarrel.geometry.vertices[6].z = 0;

  this.tankbarrel.geometry.vertices[7].x = -3;
  this.tankbarrel.geometry.vertices[7].y = -3;
  this.tankbarrel.geometry.vertices[7].z = 56;

  this.tankbarrel.position.y = 25;
  this.tankbarrel.position.z = 14;
  //scene.add(tankbarrel);

  this.tankturret.add(this.tankbarrel);
  this.tankbody.add(this.tankturret);

  //this.tankbody.useQuaternion = true;

  scene.add(this.tankbody);
}

function setTank(id, x, /*y, */z, r, tr, br, v, ts, tts, bts) {
  for (var i in tanks)
  {
    if (tanks[i].id == id)
    {
      tanks[i].tankbody.position.x = x;
    //  tanks[i].tankbody.position.y = y;
      tanks[i].tankbody.position.z = z;
      tanks[i].tankbody.rotation.y = r;
      tanks[i].tankturret.rotation.y = tr;
      tanks[i].tankbarrel.rotation.x = br;
      tanks[i].velocity = v;
      tanks[i].turnspeed = ts;
      tanks[i].turret.turnspeed = tts;
      tanks[i].barrel.turnspeed = bts;

      var n = getLandscapeNormal(i);
      var q1 = new THREE.Quaternion();
      q1.setFromAxisAngle(new THREE.Vector3(0, 1, 0), tanks[i].tankbody.rotation.y);
      var q2 = new THREE.Quaternion();
      var axis = new THREE.Vector3(0, 1, 0);
      var theta = Math.acos(axis.dot(n));
      axis.cross(n).normalize();
      q2.setFromAxisAngle(axis, theta);
      q2.multiply(q1);
      tanks[i].tankbody.quaternion = q2;
      break;
    }
  }
}
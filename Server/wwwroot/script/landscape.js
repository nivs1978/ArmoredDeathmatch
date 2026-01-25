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

var landscapescale = 20000;
var lsize = 256;
var texture1;

var skidmarkscanvas = document.createElement('canvas');
skidmarkscanvas.width = 1024;
skidmarkscanvas.height = 1024;
var ctx = skidmarkscanvas.getContext('2d');
/*
ctx.fillStyle = "rgba(200,0,0)";
ctx.fillRect(10, 10, 185, 170);

ctx.fillStyle = "rgba(0, 0, 200, 0.5)";
ctx.fillRect(40, 50, 755, 950);
*/
function addLandscape(ls) {
  landscape = ls;
  //landscape = generateLandscape(lsize + 1);

  texture1 = new THREE.Texture(skidmarkscanvas);
  texture1.needsUpdate = true;

  var texture2 = THREE.ImageUtils.loadTexture("graphics/landscape.jpg");
  texture2.repeat.x = 32;
  texture2.repeat.y = 32;
  texture2.wrapT = THREE.RepeatWrapping;
  texture2.wrapS = THREE.RepeatWrapping;
  texture2.needsUpdate = true;

  var material1 = new THREE.MeshLambertMaterial({ ambient: 0xffffff, transparent: true, map: texture1 });
  var material2 = new THREE.MeshLambertMaterial({ ambient: 0xffffff, map: texture2 });
/*  material1.polygonOffset = true;
  material1.polygonOffsetFactor = -1;*/
/*  material1.depthTest = false;
  material1.depthWrite = false;*/

  var materials = [material1, material2];

  //var material = new THREE.MeshLambertMaterial({ color: 0xffffff, map: texture });
  var plane = new THREE.PlaneGeometry(2 * landscapescale, 2 * landscapescale, lsize, lsize);

  for (var i = 0, l = plane.vertices.length; i < l; i++) {

    var x = i % (lsize + 1)
    var y = ~ ~(i / (lsize + 1));
    //landscape[x][y] = 0; //REMOVE THIS
    plane.vertices[i].z = landscape[x][y];
  }
  //plane.computeCentroids();

  var mesh = THREE.SceneUtils.createMultiMaterialObject(plane, materials);
  mesh.rotation.x = -90 * Math.PI / 180;
  scene.add(mesh);
}

function getLandscapeNormal(id) {
  var vidx = [11, 10, 5, 4]; // The four vertexes in the tankbody to use as height measure points 
  var p = [];
  for (var i = 0; i < 4; i++) {
    var r = tanks[id].rotation;
    var ox = tanks[id].tankbody.geometry.vertices[vidx[i]].z;
    var oz = tanks[id].tankbody.geometry.vertices[vidx[i]].x;
    var sx = tanks[id].tankbody.position.x + Math.sin(r) * ox + Math.cos(r) * oz;
    var sz = tanks[id].tankbody.position.z + Math.cos(r) * ox - Math.sin(r) * oz;
    var gx = toHeightMapX(sx);
    var gz = toHeightMapZ(sz);
    p[i] = new THREE.Vector3(sx, pointHeight(gx, gz), sz);
  }

  var N = new THREE.Vector3(
        (p[1].y - p[2].y) * (p[1].z - p[3].z) - (p[1].z - p[2].z) * (p[1].y - p[3].y),
        -((p[1].z - p[2].z) * (p[1].x - p[3].x) - (p[1].x - p[2].x) * (p[1].z - p[3].z)),
        (p[1].x - p[2].x) * (p[1].y - p[3].y) - (p[1].y - p[2].y) * (p[1].x - p[3].x)
      );
  N = N.normalize();
  N.x = -N.x;
  N.z = -N.z;
  return N;
}

function getHeight(px, pz, x1, z1, x2, z2, x3, z3) {
  var h1 = landscape[x1][z1];
  var h2 = landscape[x2][z2];
  var h3 = landscape[x3][z3];
  var diffx = h1 - h2;
  var diffz = h3 - h2;
  var h = px * diffx + pz * diffz;
  return h2 + h;
}

function pointHeight(_x, _z) {
  var h = 0;
  var x = Math.floor(_x);
  var z = Math.floor(_z);
  var tx = _x - x;
  var tz = _z - z;
  var d = document.getElementById("infowindow");
  if (tx + tz > 1) { // top right triangle
    h = getHeight(1 - Math.abs(_x - x), 1 - Math.abs(_z - z), x, z + 1, x + 1, z + 1, x + 1, z);
  } else { // bottom left triangle
    h = getHeight(Math.abs(_x - x), Math.abs(_z - z), x + 1, z, x, z, x, z + 1);
  }
  return h;
}

function toHeightMapX(x) {
  x = x + landscapescale;
  x = x / ((landscapescale * 2) / lsize);
  return x;
}

function toHeightMapZ(z) {
  z = z + landscapescale;
  z = z / ((landscapescale * 2) / lsize);
  return z;
}

function toSkidmarksX(x) {
  x += landscapescale;
  x = x / (landscapescale/512.0);
  return x;
}

function toSkidmarksZ(z) {
  z += landscapescale;
  z = z / (landscapescale/512.0);
  return z;
}

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

var TimeToFade = 1000.0;

function fade(eid) {
  var element = document.getElementById(eid);
  if (element == null)
    return;

  if (element.FadeState == null) {
    if (element.style.opacity == null
        || element.style.opacity == ''
        || element.style.opacity == '1') {
      element.FadeState = 2;
    }
    else {
      element.FadeState = -2;
    }
  }

  if (element.FadeState == 1 || element.FadeState == -1) {
    element.FadeState = element.FadeState == 1 ? -1 : 1;
    element.FadeTimeLeft = TimeToFade - element.FadeTimeLeft;
  }
  else {
    element.FadeState = element.FadeState == 2 ? -1 : 1;
    element.FadeTimeLeft = TimeToFade;
    setTimeout("animateFade(" + new Date().getTime() + ",'" + eid + "')", 33);
  }
}

function animateFade(lastTick, eid) {
  var curTick = new Date().getTime();
  var elapsedTicks = curTick - lastTick;

  var element = document.getElementById(eid);

  if (element.FadeTimeLeft <= elapsedTicks) {
    element.style.opacity = element.FadeState == 1 ? '1' : '0';
    element.style.filter = 'alpha(opacity = '
        + (element.FadeState == 1 ? '100' : '0') + ')';
    element.FadeState = element.FadeState == 1 ? 2 : -2;
    return;
  }

  element.FadeTimeLeft -= elapsedTicks;
  var newOpVal = element.FadeTimeLeft / TimeToFade;
  if (element.FadeState == 1)
    newOpVal = 1 - newOpVal;

  element.style.opacity = newOpVal;
  element.style.filter = 'alpha(opacity = ' + (newOpVal * 100) + ')';

  setTimeout("animateFade(" + curTick + ",'" + eid + "')", 33);
}
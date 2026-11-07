'use strict';
// Teaching model only. No Unity state, saves, NavMesh or source files are read or changed.
const $=id=>document.getElementById(id);
const defaults=()=>({running:false,mode:'ai',pos:-12,enemy:0,dir:1,awareness:0,health:100,battery:100,progress:0,recording:false,goals:[false,false,false,false],checkpoint:null,message:'Simulació aturada.'});
let state=defaults(),world=null,last=0,lastText='',lastReadout=0;
function message(text){state.message=text;}
function show(){
 const detection=state.awareness>=1?'PERSECUCIÓ':state.awareness>0?'SOSPITA / CERCA':'PATRULLA';
 const text=`${state.running?'EN MARXA':'ATURAT'} · ${detection}\nSospita: ${Math.round(state.awareness*100)}% · vida: ${Math.round(state.health)} · bateria: ${Math.round(state.battery)}%\n${state.message}`;
 if(text!==lastText){$('status').textContent=text;lastText=text;}
 $('posValue').value=`${state.pos.toFixed(1)} m`;
 $('evidence').value=state.progress;
 $('objectives').textContent=['Piles','Energia','Anomalia','Sortida'].map((g,i)=>(state.goals[i]?'✓ ':'○ ')+g).join(' → ');
 $('play').textContent=state.running?'Aturar simulació':'Iniciar simulació';$('play').setAttribute('aria-pressed',String(state.running));
 $('record').setAttribute('aria-pressed',String(state.recording));$('record').textContent=state.recording?'Aturar gravació simulada':'Començar gravació simulada';
 $('capture').disabled=$('difficulty').value==='nightmare';
 document.querySelectorAll('[data-objective]').forEach(b=>{b.disabled=b.dataset.objective==='power'?!state.goals[0]:b.dataset.objective==='exit'?!state.goals[2]:state.goals[0];});
}
function reset(){state=defaults();$('position').value=-12;for(const id of ['camera','night','crouch','hidden','occluded'])$(id).checked=false;$('flashlight').checked=true;$('difficulty').value='standard';show();}
$('play').onclick=()=>{state.running=!state.running;show();};$('reset').onclick=reset;
$('position').oninput=e=>{state.pos=Number(e.target.value);show();};
document.querySelectorAll('[data-mode]').forEach(button=>button.onclick=()=>{state.mode=button.dataset.mode;state.recording=false;for(const mode of ['ai','camera','checkpoint']){$('panel-'+mode).hidden=mode!==state.mode;$('tab-'+mode).setAttribute('aria-selected',String(mode===state.mode));}show();});
document.querySelector('[role="tablist"]').addEventListener('keydown',event=>{const tabs=[...document.querySelectorAll('[data-mode]')];let i=tabs.indexOf(document.activeElement);if(i<0||!['ArrowLeft','ArrowRight','Home','End'].includes(event.key))return;event.preventDefault();i=event.key==='Home'?0:event.key==='End'?tabs.length-1:(i+(event.key==='ArrowRight'?1:-1)+tabs.length)%tabs.length;tabs[i].focus();tabs[i].click();});
$('record').onclick=()=>{state.recording=!state.recording;if(!state.running)message('Inicia la simulació per avançar el temps de gravació.');show();};
$('reload').onclick=()=>{state.battery=100;message('Bateria afegida al model. Al joc cal un consumible de càmera.');show();};
$('capture').onclick=()=>{if($('difficulty').value==='nightmare')return;state.checkpoint={pos:state.pos,health:state.health,battery:state.battery,goals:[...state.goals]};message('Checkpoint capturat: mou-te o aplica dany i prova el retorn.');show();};
$('damage').onclick=()=>{state.health=Math.max(0,state.health-35);message('Dany aplicat al model.');show();};
$('respawn').onclick=()=>{if($('difficulty').value==='nightmare'||!state.checkpoint){message('Sense retorn: Nightmare o cap checkpoint capturat.');}else{const cp=state.checkpoint;state.pos=cp.pos;state.health=cp.health;state.battery=cp.battery;state.goals=[...cp.goals];state.progress=state.goals[2]?1:0;state.awareness=0;state.recording=false;$('position').value=state.pos;message('Estat restaurat al checkpoint del laboratori.');}show();};
$('difficulty').onchange=()=>{if($('difficulty').value==='nightmare')state.checkpoint=null;message('Perfil del model actualitzat. Les xifres no reprodueixen els multiplicadors Unity.');show();};
document.querySelectorAll('[data-objective]').forEach(button=>button.onclick=()=>{const id=button.dataset.objective;if(id==='battery'){state.goals[0]=true;message('Piles recollides: generador disponible.');}if(id==='power'&&state.goals[0]){state.goals[1]=true;message('Energia restaurada: prepara la càmera i grava el subjecte.');}if(id==='exit'&&state.goals[2]){state.goals[3]=true;message('Has escapat de la instal·lació (final simulat).');state.running=false;}show();});
function step(dt){
 if(!state.running||document.hidden)return;
 if(state.mode==='ai'){
  const distance=Math.abs(state.enemy-state.pos);
  const visible=!$('hidden').checked&&!$('occluded').checked&&distance<($('flashlight').checked?9:5);
  state.awareness=Math.max(0,Math.min(1,state.awareness+(visible?($('crouch').checked?.2:.5):-.35)*dt));
  if(state.awareness>=1){state.enemy+=Math.sign(state.pos-state.enemy)*Math.min(distance,dt*2.2);}else{state.enemy+=state.dir*dt*1.1;if(state.enemy>7)state.dir=-1;if(state.enemy<-7)state.dir=1;}
 }else if(state.mode==='camera'){
  const active=$('camera').checked&&state.battery>0;
  if(active&&$('night').checked)state.battery=Math.max(0,state.battery-dt*2);
  if(state.recording&&!state.goals[2]){
   const valid=active&&state.goals[1]&&Math.abs(state.pos-11)<=14;
   if(valid){state.progress=Math.min(1,state.progress+dt/2.5);if(state.progress>=1){state.goals[2]=true;state.recording=false;message('Evidència arxivada: la sortida ja és disponible.');}}
   else{state.progress=0;message(!state.goals[1]?'Cal restaurar energia abans de registrar aquest objectiu.':'Equipa la càmera, comprova bateria i acosta’t a 14 m del subjecte.');}
  }else if(!state.goals[2])state.progress=0;
 }
}
function mount(){
 if(typeof THREE==='undefined'){$('sceneCaption').textContent='Three.js no s’ha carregat. Cal Internet per al CDN; les simulacions de dades i la guia continuen disponibles.';return;}
 try{
  const el=$('stage'),scene=new THREE.Scene();scene.background=new THREE.Color(0x081312);
  const camera=new THREE.PerspectiveCamera(48,1,.1,160);camera.position.set(23,26,29);camera.lookAt(0,0,0);
  const renderer=new THREE.WebGLRenderer({antialias:true});renderer.setPixelRatio(Math.min(devicePixelRatio||1,2));el.prepend(renderer.domElement);renderer.domElement.setAttribute('aria-hidden','true');
  scene.add(new THREE.HemisphereLight(0xb7e7cb,0x142019,1.2));const light=new THREE.DirectionalLight(0xffffff,.8);light.position.set(0,18,8);scene.add(light);
  const box=(x,y,z,color)=>new THREE.Mesh(new THREE.BoxGeometry(x,y,z),new THREE.MeshLambertMaterial({color}));
  const floor=box(32,.3,7,0x30483d);floor.position.y=-.2;scene.add(floor);
  for(const z of [-3.6,3.6]){const wall=box(32,1.5,.22,0x1d3932);wall.position.set(0,.5,z);scene.add(wall);}
  const player=box(.75,1.7,.75,0x74b8f0),enemy=box(.9,1.9,.9,0xe78373),subject=box(.8,2,.8,0xa5e9a4);scene.add(player,enemy,subject);subject.position.set(11,1,0);
  const checkpoint=box(.12,.1,6,0xe1c572);scene.add(checkpoint);checkpoint.visible=false;
  const beam=new THREE.Mesh(new THREE.ConeGeometry(2.4,6,24,1,true),new THREE.MeshBasicMaterial({color:0xe7e6a1,transparent:true,opacity:.14,side:THREE.DoubleSide,depthWrite:false}));beam.rotation.z=Math.PI/2;scene.add(beam);
  const resize=()=>{const w=el.clientWidth,h=el.clientHeight;renderer.setSize(w,h,false);camera.aspect=w/h;camera.updateProjectionMatrix();};
  new ResizeObserver(resize).observe(el);resize();world={renderer,scene,camera,player,enemy,subject,checkpoint,beam};
 }catch(error){$('sceneCaption').textContent='WebGL no disponible; pots usar els controls i llegir la guia sense la maqueta 3D.';}
}
function draw(){if(!world||document.hidden)return;const w=world;w.player.position.set(state.pos,.8,0);w.player.scale.y=$('crouch').checked?.6:1;w.player.visible=!$('hidden').checked;w.enemy.position.set(state.enemy,.9,1.4);w.enemy.visible=state.mode==='ai';w.subject.material.color.setHex(state.goals[2]?0xf3d483:0xa5e9a4);w.beam.position.set(state.pos+3,.8,0);w.beam.visible=state.mode==='ai'&&$('flashlight').checked&&!$('hidden').checked;w.checkpoint.visible=Boolean(state.checkpoint);if(state.checkpoint)w.checkpoint.position.set(state.checkpoint.pos,.02,0);w.scene.background.setHex(state.mode==='camera'&&$('night').checked?0x18391e:0x081312);w.renderer.render(w.scene,w.camera);}
function frame(time){const dt=last?Math.min(.1,(time-last)/1000):0;last=time;step(dt);draw();if(time-lastReadout>300){show();lastReadout=time;}requestAnimationFrame(frame);}
document.addEventListener('visibilitychange',()=>{last=0;});
mount();show();requestAnimationFrame(frame);

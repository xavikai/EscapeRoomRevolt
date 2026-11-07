#!/usr/bin/env node
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto'),assert=require('node:assert/strict');
process.chdir(path.resolve(__dirname,'..'));
const layout=require('./workspace-layout.cjs');
function files(d){return fs.readdirSync(d,{withFileTypes:true}).flatMap(e=>e.isDirectory()?files(d+'/'+e.name):[d+'/'+e.name]);}
const json=p=>JSON.parse(fs.readFileSync(p,'utf8'));
const hash=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const shared=path.resolve('SharedPackages/com.xavikai.revolt.common');
for(const name of ['EscapeRoom','Survival']){
 const root=layout.productPath(name),manifest=json(root+'/Packages/manifest.json'),inventory=json(root+'/product-inventory.json');
 const common=path.resolve(root,'Packages',manifest.dependencies['com.xavikai.revolt.common'].slice(5));
 assert.equal(fs.realpathSync(common),fs.realpathSync(shared),'Consumers must share the canonical Common source.');
 assert.equal(!!manifest.dependencies['com.xavikai.revolt.survival'],name==='Survival');
 assert.ok(!manifest.dependencies['com.coplaydev.unity-mcp']);
 const lock=json(root+'/Packages/packages-lock.json');assert.ok(!lock.dependencies['com.coplaydev.unity-mcp']);
 const assets=files(root+'/Assets/_EscapeRoomTemplate');
 assert.ok(fs.existsSync(root+'/Assets/TextMesh Pro/Resources/TMP Settings.asset'),'Implicit TMP settings must be shipped with each consumer.');
 assert.ok(!assets.some(p=>p.endsWith('.cs')),'Framework code must not be duplicated into consumer Assets.');
 assert.deepEqual(assets.filter(p=>p.endsWith('.unity')).map(p=>path.basename(p,'.unity')).sort(),[...inventory.scenes].sort());
 if(name==='EscapeRoom'){
  assert.ok(!assets.some(p=>p.endsWith('/SurvivalDifficultySettings.asset')));
  assert.ok(!assets.some(p=>p.includes('/ScriptableObjects/Survival/')));
  assert.ok(!lock.dependencies['com.unity.ai.navigation']);
 }else {
  const scene='Assets/_EscapeRoomTemplate/Scenes/SurvivalHorrorDemo.unity';
  assert.equal(fs.readFileSync(root+'/'+scene+'.meta','utf8').match(/guid: (\w+)/)[1],fs.readFileSync(scene+'.meta','utf8').match(/guid: (\w+)/)[1],'Preserve Survival demo scene identity.');
  if(hash(root+'/'+scene)!==hash(scene)){
   const nav='Assets/_EscapeRoomTemplate/Scenes/Navigation/SurvivalHorrorDemo_NavMesh.asset';
   assert.ok(fs.existsSync(root+'/'+nav),'A repaired demo must carry its persistent navigation data.');
   const navGuid=fs.readFileSync(root+'/'+nav+'.meta','utf8').match(/guid: (\w+)/)[1];
   assert.ok(fs.readFileSync(root+'/'+scene,'utf8').includes(navGuid),'Repaired scene must reference the navigation asset.');
  }
 }
 const packageDirs=Object.keys(manifest.dependencies).filter(p=>p.startsWith('com.xavikai.')).map(p=>'SharedPackages/'+p);
 const metas=[...files(root+'/Assets'),...packageDirs.flatMap(files)].filter(p=>p.endsWith('.meta'));
 const guids=new Map();for(const file of metas){const guid=fs.readFileSync(file,'utf8').match(/^guid: (\w+)/m)?.[1];if(!guid)continue;assert.ok(!guids.has(guid),'Duplicate GUID: '+file+' and '+guids.get(guid));guids.set(guid,file);}
 console.log(name+': canonical source, isolated resources/scenes, scene provenance and '+guids.size+' unique GUIDs PASS.');
}
const hud=fs.readFileSync(shared+'/UI/Toolkit/GameplayHUD.uxml','utf8');
for(const name of ['sanity-hud','vitals-hud','camcorder-hud'])assert.ok(!hud.includes('name="'+name+'"'));
assert.ok(fs.existsSync('SharedPackages/com.xavikai.revolt.survival/UI/Resources/SurvivalHUD.uxml'));
for(const [previous,current]of Object.entries(json('SharedPackages/source-map.json')))assert.ok(fs.existsSync(current),'Missing migration destination: '+previous+' -> '+current);
console.log('Shared templates and migration map PASS.');

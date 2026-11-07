#!/usr/bin/env node
// Create new consumer projects from the authoring data. Refuses to overwrite existing projects.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const repo=path.resolve(__dirname,'..');process.chdir(repo);
const layout=require('./workspace-layout.cjs');
const base='Assets/_EscapeRoomTemplate/';
const products=[
 {folder:'EscapeRoom',id:'escaperoom-revolt',title:'Escape Room Revolt',genre:0,applicationId:'com.xavikai.escaperoomrevolt',first:'ShowcaseMuseum',scenes:['Intro','MainMenu','ShowcaseMuseum','LockedOffice','VRTemplate','ShowcaseMuseumVR','LockedOfficeVR'],desktop:['Intro','MainMenu','ShowcaseMuseum','LockedOffice'],quest:['ShowcaseMuseumVR'],survival:false},
 {folder:'Survival',id:'survival-revolt',title:'Survival Revolt',genre:1,applicationId:'com.xavikai.survivalrevolt',first:'SurvivalHorrorDemo',scenes:['Intro','MainMenu','SurvivalHorrorDemo','VRTemplate'],desktop:['Intro','MainMenu','SurvivalHorrorDemo'],quest:[],survival:true}
];
const staging=process.argv.includes('--staging');
const requested=process.argv.slice(2).find(arg=>!arg.startsWith('--'));const selected=requested?products.filter(p=>p.folder.toLowerCase()===requested.toLowerCase()):products;
if(!selected.length)throw Error('Usage: node tools/create-products.cjs [EscapeRoom|Survival]');
const outputRoot=process.argv.slice(2).find(arg=>arg.startsWith('--output-root='))?.slice('--output-root='.length) || (staging?'.utmp/consumer-stage':null);
const destination=p=>outputRoot?outputRoot+'/'+p.folder:layout.productPath(p.folder);
for(const p of selected)if(fs.existsSync(destination(p)))throw Error('Existing project preserved: '+destination(p)+'. Choose a fresh output; never regenerate over edited consumer data.');
function walk(d){if(!fs.existsSync(d))return[];return fs.readdirSync(d,{withFileTypes:true}).flatMap(e=>e.isDirectory()?walk(d+'/'+e.name):[d+'/'+e.name]);}
const read=p=>fs.readFileSync(p,'utf8').replace(/\r\n/g,'\n');
function write(p,s){fs.mkdirSync(path.dirname(p),{recursive:true});fs.writeFileSync(p,s);}
const assetFiles=[...walk('Assets'),...walk('SharedPackages')];
if(!fs.existsSync('tools/product-asset-dependencies.json'))throw Error('Run ProductDependencyExporter.Export in the authoring Unity project first. Binary assets require Unity dependency data.');
const exportedDependencies=new Map(JSON.parse(read('tools/product-asset-dependencies.json')).entries.map(entry=>[entry.asset,entry.dependencies.map(p=>p.replace(/^Packages\/com\.xavikai\./,'SharedPackages/com.xavikai.'))]));
const isBinary=file=>fs.readFileSync(file).subarray(0,256).includes(0);
const byGuid=new Map();
for(const p of assetFiles.filter(p=>p.endsWith('.meta'))){const guid=read(p).match(/^guid: (\w+)/m)?.[1];if(guid)byGuid.set(guid,p.slice(0,-5));}
const textExtensions=/\.(asset|prefab|unity|mat|controller|overrideController|anim|inputactions|uxml|uss|meta|asmdef)$/i;
const catalogue=base+'ScriptableObjects/DefaultItemCatalog.asset';
const itemScriptGuid=read('SharedPackages/com.xavikai.revolt.common/Systems/Inventory/InventoryItemData.cs.meta').match(/guid: (\w+)/)[1];
const configScriptGuid=read('SharedPackages/com.xavikai.revolt.common/Core/Runtime/Settings/ProductConfiguration.cs.meta').match(/guid: (\w+)/)[1];
const excludedEscapeScripts=['SanityController','HorrorEventTrigger'].map(name=>read('SharedPackages/com.xavikai.revolt.survival/'+name+'.cs.meta').match(/guid: (\w+)/)[1]);
function sourceText(file,product){
 let text=read(file);
 if(!product||product.survival||!file.endsWith('.unity'))return text;
 const removed=[];
 text=text.replace(/--- !u!114 &(\d+)[\s\S]*?(?=\n--- !u!|$)/g,(block,id)=>{
  if(excludedEscapeScripts.some(guid=>block.includes('guid: '+guid))){removed.push(id);return '';}
  return block;
 });
 for(const id of removed){
  text=text.replace(new RegExp('^  - component: \\{fileID: '+id+'\\}\\n','gm'),'');
  text=text.replace(new RegExp('^    - targetCorrespondingSourceObject: [^\\n]*\\n      insertIndex: [^\\n]*\\n      addedObject: \\{fileID: '+id+'\\}\\n','gm'),'');
 }
 return text.replace(/    m_AddedComponents:\n(?=  m_SourcePrefab:)/g,'    m_AddedComponents: []\n');
}
function references(file,product){
 let contents='';if(textExtensions.test(file)&&!isBinary(file))contents=sourceText(file,product);
 if(file===catalogue)contents=contents.slice(0,contents.indexOf('  _items:'));
 if(fs.existsSync(file+'.meta'))contents+='\n'+read(file+'.meta');
 const dependencies=[...contents.matchAll(/guid:\s*([a-f\d]{32})/gi)].map(m=>byGuid.get(m[1])).filter(Boolean);
 if(isBinary(file))dependencies.push(...(exportedDependencies.get(file)||[]));
 return dependencies;
}
function collect(seeds,product){const included=new Set(),todo=[...seeds];while(todo.length){const p=todo.pop();if(included.has(p)||!fs.existsSync(p)||!fs.statSync(p).isFile())continue;included.add(p);todo.push(...references(p,product));}return included;}
const sharedPackages=['com.xavikai.revolt.common','com.xavikai.revolt.survival'];
for(const product of selected){
 const dest=destination(product);
 const seeds=product.scenes.map(s=>base+'Scenes/'+s+'.unity');
 seeds.push(base+'Prefabs/GameManager.prefab',base+'Prefabs/Player_PC.prefab',base+'Prefabs/Player_VR.prefab',base+'Resources/GenreFeatureSettings.asset',base+'Resources/GameFlowSettings.asset',base+'Resources/VRComfortSettings.asset',catalogue);
 // TMP loads these global defaults by Resources name; serialized scene GUID closure cannot discover them.
 seeds.push('Assets/TextMesh Pro/Resources/TMP Settings.asset');
 if(product.survival){seeds.push(base+'Resources/SurvivalDifficultySettings.asset');seeds.push(...walk(base+'Prefabs/Survival').filter(p=>!p.endsWith('.meta')),...walk(base+'ScriptableObjects/Survival').filter(p=>!p.endsWith('.meta')));}
 // Input/XR samples are imported authoring dependencies of the generated rigs; retain their asmdefs and notices together.
 seeds.push(...walk('Assets/Samples').filter(p=>!p.endsWith('.meta')),...walk('Assets/XR').filter(p=>!p.endsWith('.meta')),...walk('Assets/XRI').filter(p=>!p.endsWith('.meta')));
 for(const p of walk('ProjectSettings').filter(p=>p.endsWith('.asset')&&!p.endsWith('/EditorBuildSettings.asset')))seeds.push(...references(p));
 const included=collect(seeds,product);
 if(!product.survival){const illegal=[...included].filter(p=>p.startsWith('SharedPackages/com.xavikai.revolt.survival/'));if(illegal.length)throw Error('Escape Room still requires Survival: '+illegal.join(', '));}
 const owned=[...included].filter(p=>p.startsWith('Assets/')).sort();
 function copyAsset(source){const target=dest+'/'+source;fs.mkdirSync(path.dirname(target),{recursive:true});if(source.endsWith('.unity')&&!isBinary(source))write(target,sourceText(source,product));else fs.copyFileSync(source,target);if(fs.existsSync(source+'.meta'))fs.copyFileSync(source+'.meta',target+'.meta');let parent=path.posix.dirname(source);while(parent!=='Assets'&&parent.startsWith('Assets/')){if(fs.existsSync(parent+'.meta')){fs.mkdirSync(dest+'/'+path.posix.dirname(parent),{recursive:true});fs.copyFileSync(parent+'.meta',dest+'/'+parent+'.meta');}parent=path.posix.dirname(parent);}}
 for(const p of owned)copyAsset(p);
 // Preserve notices and manuals as project-local content; they are not package source code.
 for(const p of walk(base).filter(p=>/\.md$/.test(p)&&(!p.includes('SURVIVAL_HORROR_')||product.survival)))copyAsset(p);
 for(const p of assetFiles.filter(p=>p.startsWith('Assets/')&&/license.*\.txt$/i.test(p)))copyAsset(p);
 for(const p of walk('ProjectSettings')){fs.mkdirSync(dest+'/'+path.dirname(p),{recursive:true});fs.copyFileSync(p,dest+'/'+p);}
 const graphicsPath=dest+'/ProjectSettings/GraphicsSettings.asset';
 let graphics=read(graphicsPath);
 for(const shader of ['Systems/Options/FlashlightVolumetricCone.shader','Systems/Interaction/SelectionMask.shader','Systems/Interaction/SelectionOutlineComposite.shader']){
  const guid=read('SharedPackages/com.xavikai.revolt.common/'+shader+'.meta').match(/guid: (\w+)/)[1];
  if(!graphics.includes(guid))graphics=graphics.replace('  m_PreloadedShaders:','  - {fileID: 4800000, guid: '+guid+', type: 3}\n  m_PreloadedShaders:');
 }
 write(graphicsPath,graphics);
 let settings=read(dest+'/ProjectSettings/ProjectSettings.asset');
 settings=settings.replace(/^  productGUID:.*$/m,'  productGUID: '+crypto.createHash('md5').update(product.id).digest('hex')).replace(/^  productName:.*$/m,'  productName: '+product.title).replace(/^(    Android: )com\.xavikai\.[^\r\n]+/m,'$1'+product.applicationId);
 write(dest+'/ProjectSettings/ProjectSettings.asset',settings);
 const sceneLines=product.scenes.flatMap(s=>{const p=base+'Scenes/'+s+'.unity',g=read(p+'.meta').match(/guid: (\w+)/)[1];return ['  - enabled: 1','    path: '+p,'    guid: '+g];}).join('\n');
 write(dest+'/ProjectSettings/EditorBuildSettings.asset',read('ProjectSettings/EditorBuildSettings.asset').replace(/  m_Scenes:[\s\S]*?(?=  m_configObjects:)/,'  m_Scenes:\n'+sceneLines+'\n'));
 const deps={...JSON.parse(read('Packages/manifest.json')).dependencies};delete deps['com.coplaydev.unity-mcp'];delete deps['com.unity.multiplayer.center'];
 if(!product.survival){delete deps['com.xavikai.revolt.survival'];delete deps['com.unity.ai.navigation'];}
 for(const name of sharedPackages)if(deps[name])deps[name]='file:'+path.relative(path.resolve(dest,'Packages'),path.resolve('SharedPackages',name)).replaceAll('\\','/');
 const testables=product.survival?sharedPackages:[sharedPackages[0]];
 write(dest+'/Packages/manifest.json',JSON.stringify({dependencies:deps,testables},null,2)+'\n');
 // A local file reference shares source; exact dependency versions and the committed lock pin external packages.
 const genre=dest+'/'+base+'Resources/GenreFeatureSettings.asset';write(genre,read(genre).replace(/  _genre: \d+/,'  _genre: '+product.genre));
 const flow=dest+'/'+base+'Resources/GameFlowSettings.asset';write(flow,read(flow).replace(/  _firstGameplayScene: .*/,'  _firstGameplayScene: '+product.first));
 const itemGuids=owned.filter(p=>p.endsWith('.asset')&&read(p).includes('guid: '+itemScriptGuid)).map(p=>read(p+'.meta').match(/guid: (\w+)/)[1]);
 write(dest+'/'+catalogue,read(catalogue).replace(/  _items:[\s\S]*/,'  _items:'+(itemGuids.length?'\n'+itemGuids.map(g=>'  - {fileID: 11400000, guid: '+g+', type: 2}').join('\n'):' []')+'\n'));
 const asset=base+'Resources/ProductConfiguration.asset';
 const sceneList=(key,scenes)=>'  '+key+':'+(scenes.length?'\n'+scenes.map(s=>'  - '+base+'Scenes/'+s+'.unity').join('\n'):' []');
 write(dest+'/'+asset,'%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!114 &11400000\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: 0}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n  m_Script: {fileID: 11500000, guid: '+configScriptGuid+', type: 3}\n  m_Name: ProductConfiguration\n  m_EditorClassIdentifier: EscapeRoomRevolt.Core::EscapeRoomRevolt.Core.Settings.ProductConfiguration\n  _productId: '+product.id+'\n  _displayName: '+product.title+'\n  _applicationIdentifier: '+product.applicationId+'\n'+sceneList('_desktopScenes',product.desktop)+'\n'+sceneList('_questScenes',product.quest)+'\n');
 write(dest+'/'+asset+'.meta','fileFormatVersion: 2\nguid: '+crypto.createHash('md5').update(product.id+'/configuration').digest('hex')+'\nNativeFormatImporter:\n  externalObjects: {}\n  mainObjectFileID: 11400000\n');
 write(dest+'/README.md','# '+product.title+'\n\nOpen this directory in Unity 6000.4.9f1. Keep it inside the repository: Packages/manifest.json references the canonical Framework/SharedPackages through local UPM dependencies.\n\nSource code is owned by the shared packages; edit gameplay data and scenes in this project independently. Default scene: '+product.first+'. PC release uses ProductConfiguration. '+(product.quest.length?'Quest demo: ShowcaseMuseumVR.':'A Survival Quest demo has not been validated; the Quest builder intentionally rejects this configuration.')+'\n\nPlayer settings and save slots are isolated by product name, application identifier and product data namespace. See the technical documentation in EscapeRoom/Framework/docs/SEPARACIO_PROJECTES.md.\n');
 const inventory={productId:product.id,unityVersion:'6000.4.9f1',packages:testables.map(name=>({name,version:JSON.parse(read('SharedPackages/'+name+'/package.json')).version})),scenes:product.scenes,firstGameplayScene:product.first,itemCount:itemGuids.length,assets:owned};
 write(dest+'/product-inventory.json',JSON.stringify(inventory,null,2)+'\n');
 console.log(product.folder+': '+owned.length+' owned assets, '+itemGuids.length+' catalog items, '+testables.length+' source packages.');
}

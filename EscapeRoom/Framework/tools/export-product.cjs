#!/usr/bin/env node
// Produce a relocatable snapshot with embedded source packages; never overwrite a prior export.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const repo=path.resolve(__dirname,'..');process.chdir(repo);
const layout=require('./workspace-layout.cjs');
const name=process.argv[2];if(!['EscapeRoom','Survival'].includes(name))throw Error('Usage: node tools/export-product.cjs EscapeRoom|Survival [destination]');
const source=layout.productPath(name),output=process.argv[3]?path.resolve(layout.workspace,process.argv[3]):layout.exportPath(name);
if(fs.existsSync(output))throw Error('Existing export preserved: '+output);
if(output===path.resolve(source)||output.startsWith(path.resolve(source)+path.sep))throw Error('Export must be outside the consumer project.');
function copy(from,to){fs.mkdirSync(path.dirname(to),{recursive:true});fs.cpSync(from,to,{recursive:true,filter:p=>!/(?:^|[\\/])(?:Library|Temp|\.utmp|InitTestScene[^\\/]*|SceneDependencyCache[^\\/]*)$/.test(p)});}
for(const part of ['Assets','ProjectSettings','README.md','product-inventory.json','docs'])if(fs.existsSync(source+'/'+part))copy(source+'/'+part,output+'/'+part);
const manifest=JSON.parse(fs.readFileSync(source+'/Packages/manifest.json','utf8'));
const packages=[];
for(const name of Object.keys(manifest.dependencies).filter(name=>name.startsWith('com.xavikai.revolt.'))){
 copy('SharedPackages/'+name,output+'/Packages/'+name);
 manifest.dependencies[name]='file:'+name;
 const packageJson=fs.readFileSync('SharedPackages/'+name+'/package.json');
 packages.push({name,version:JSON.parse(packageJson).version,manifestSha256:crypto.createHash('sha256').update(packageJson).digest('hex')});
}
fs.mkdirSync(output+'/Packages',{recursive:true});
fs.writeFileSync(output+'/Packages/manifest.json',JSON.stringify(manifest,null,2)+'\n');
// Keep external resolution pinned; Unity will recalculate the embedded package entries on import.
const lock=JSON.parse(fs.readFileSync(source+'/Packages/packages-lock.json','utf8'));
for(const packageEntry of packages)delete lock.dependencies[packageEntry.name];
fs.writeFileSync(output+'/Packages/packages-lock.json',JSON.stringify(lock,null,2)+'\n');
fs.writeFileSync(output+'/EXPORT.json',JSON.stringify({product:name,packages,sourceLayout:'Embedded UPM packages; no reference to the source repository. No Library, logs, builds or private MCP tools included.'},null,2)+'\n');
fs.writeFileSync(output+'/README.md','# '+name+' — portable Unity project\n\nOpen this folder with Unity 6000.4.9f1. Framework packages are embedded under Packages and require no access to the source repository. External Unity dependencies resolve through the included manifest and lock. See docs/SEPARACIO_PROJECTES.md for product configuration and EXPORT.json for package versions. This is a delivery snapshot; maintain source changes in the canonical repository packages.\n');
console.log(output);

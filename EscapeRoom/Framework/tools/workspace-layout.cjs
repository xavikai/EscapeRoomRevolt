const path=require('node:path');
const framework=path.resolve(__dirname,'..');
const workspace=path.resolve(framework,'../..');
const productRoot=name=>path.join(workspace,name);
const productPath=name=>path.join(productRoot(name),'Unity');
const exportPath=name=>path.join(productRoot(name),'Exportacions',name+'-0.2.0-pcfix-2026-10-26');
function historicalPath(value){
 const normalized=value.replaceAll('\\','/');
 let match=normalized.match(/^Projects\/(EscapeRoom|Survival)(?:\/(.*))?$/);
 if(match)return path.join(productPath(match[1]),match[2]||'');
 match=normalized.match(/^Distributions\/(?:(Deliveries|WindowsDemos)\/)?((EscapeRoom|Survival)-.*)$/);
 if(match)return path.join(productRoot(match[3]),match[1]==='Deliveries'?'PaquetsFonts':match[1]==='WindowsDemos'?'Windows':'Exportacions',match[2]);
 return path.resolve(framework,normalized);
}
module.exports={framework,workspace,productRoot,productPath,exportPath,historicalPath};

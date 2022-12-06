echo Executing custom plugin to nested namespaces
cd plugins/nested_namespaces
npm i
node nested_namespaces.js '../../lib/toc.yml'
cd..
cd..
echo Hosting the serve
docfx build --serve
cat ../apps/fs-sqlite/web.settings | sed "/debug/s/ .*/ false/" | sed "/contentRoot/s/ .*/ ..\/apps/" | sed "/webRoot/s/ .*/ ..\/apps\/fs-sqlite/" \
| sed "/port/s/ .*/ 5001/" > bin/Release/netcoreapp2.0/publish/fs-sqlite.web.settings

cat ../apps/fs-redis/web.settings | sed "/debug/s/ .*/ false/" | sed "/contentRoot/s/ .*/ ..\/apps/" | sed "/webRoot/s/ .*/ ..\/apps\/fs-redis/" \
| sed "/port/s/ .*/ 5002/" > bin/Release/netcoreapp2.0/publish/fs-redis.web.settings

rsync -avz -e 'ssh' bin/Release/netcoreapp2.0/publish/ deploy@gistlyn.com:/home/deploy/web

rsync -avz -e 'ssh' ../apps deploy@gistlyn.com:/home/deploy 

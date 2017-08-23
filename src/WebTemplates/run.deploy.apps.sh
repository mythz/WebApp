rsync -avz -e 'ssh' bin/Release/netcoreapp2.0/publish/ deploy@gistlyn.com:/home/deploy/web

cat ../apps/rockwind-fs/web.sqlite.settings | sed "/debug/s/ .*/ false/" | sed "/contentRoot/s/ .*/ ..\/apps/" | sed "/webRoot/s/ .*/ ..\/apps\/rockwind-fs/" \
| sed "/port/s/ .*/ 5001/" > bin/Release/netcoreapp2.0/publish/rockwind-fs.web.sqlite.settings

cat ../apps/rockwind-vfs/web.sqlite.settings | sed "/debug/s/ .*/ false/" | sed "/contentRoot/s/ .*/ ..\/apps/" | sed "/webRoot/s/ .*/ ..\/apps\/rockwind-vfs/" \
| sed "/port/s/ .*/ 5001/" > bin/Release/netcoreapp2.0/publish/rockwind-vfs.web.sqlite.settings

cat ../apps/redis/web.settings | sed "/debug/s/ .*/ false/" | sed "/contentRoot/s/ .*/ ..\/apps/" | sed "/webRoot/s/ .*/ ..\/apps\/redis/" \
| sed "/port/s/ .*/ 5002/" > bin/Release/netcoreapp2.0/publish/redis.web.settings

rsync -avz -e 'ssh' ../apps deploy@gistlyn.com:/home/deploy 

# run.publish.bat with cmd.exe first

cat ../apps/rockwind/web.sqlite.settings | sed "/debug/s/ .*/ false/" | sed "/port/s/ .*/ 5001/" > ../apps/bin/web.rockwind-sqlite.settings

cat ../apps/rockwind-vfs/web.sqlite.settings | sed "/debug/s/ .*/ false/" | sed "/port/s/ .*/ 5002/" > ../apps/bin/web.rockwind-vfs-sqlite.settings

cat ../apps/redis/web.settings | sed "/debug/s/ .*/ false/" | sed "/port/s/ .*/ 5003/" > ../apps/bin/web.redis.settings

cat ../apps/web-plugins/web.settings | sed "/debug/s/ .*/ false/" | sed "/port/s/ .*/ 5004/" > ../apps/bin/web.web-plugins.settings

rsync -avz -e 'ssh' ../apps deploy@gistlyn.com:/home/deploy 

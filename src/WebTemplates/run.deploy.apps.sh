# run.publish.bat with cmd.exe first

cat ../apps/rockwind/web.sqlite.settings | sed "/debug/s/ .*/ false/" | sed "/port/s/ .*/ 5001/" > ../apps/app/web.rockwind-sqlite.settings

cat ../apps/rockwind-vfs/web.sqlite.settings | sed "/debug/s/ .*/ false/" | sed "/port/s/ .*/ 5002/" > ../apps/app/web.rockwind-vfs-sqlite.settings

cat ../apps/redis/web.settings | sed "/debug/s/ .*/ false/" | sed "/port/s/ .*/ 5003/" > ../apps/app/web.redis.settings

cat ../apps/web-plugins/web.settings | sed "/debug/s/ .*/ false/" | sed "/port/s/ .*/ 5004/" > ../apps/app/web.web-plugins.settings

cat ../apps/chat/web.release.settings | sed "/port/s/ .*/ 5005/" > ../apps/app/web.chat.settings

rsync -avz -e 'ssh' ../apps deploy@gistlyn.com:/home/deploy 

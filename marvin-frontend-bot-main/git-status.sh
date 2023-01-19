cd /home/dev/Marvin-vPostgres
changed=0

cd /home/dev/Marvin-vPostgres
git fetch && git status -uno | grep -q 'Your branch is behind' && changed=1

if [ $changed = 1 ]; then
  # Changes
  echo "Changes were found"
  sh /home/dev/Marvin-vPostgres/launch.sh
else
  # No changes
  echo "No changes were found"
fi

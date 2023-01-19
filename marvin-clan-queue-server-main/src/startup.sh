if ! tmux has-session -t compute 2>/dev/null; then
  echo "starting";
  tmux new-session -d -s compute "./launch.sh";
else
  echo "session already exists";
  tmux kill-session -t compute && tmux new-session -d -s compute "./launch.sh"
fi

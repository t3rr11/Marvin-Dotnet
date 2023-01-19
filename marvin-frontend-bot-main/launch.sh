#!/bin/bash
pm2 stop 1
cd /home/dev/marvin-frontend-bot
sleep 10
cd /home/dev/marvin-frontend-bot
pm2 start 1

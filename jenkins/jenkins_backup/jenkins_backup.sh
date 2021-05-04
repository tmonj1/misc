#!/bin/bash

# set Jenkins image name and Jenkins home from which the backup is to be taken.
if [ "$1" = "master" ]; then
  JENKINS_HOME_PATH=/var/jenkins_home
  JENKINS_IMAGE=jenkins_master
elif [ "$1" = "agent" ]; then
  JENKINS_HOME_PATH=/home/jenkins
  JENKINS_IMAGE=jenkins_node_dockercli
else
  echo "usage: $0 master/agent"
  echo "  specify \"master\" or \"agent\"."
  exit 1
fi

# show which backup to take, master or agent.
echo "taking a backup of \"$1\"..."
echo "  Jenkins home path is \"$JENKINS_HOME_PATH\""
echo "  Jenkins image is \"${JENKINS_IMAGE}\""
echo ""

# Take a backup and save it in the current directory using jenkins agent image.
if [ "$1" = "master" ]; then
  docker run --volumes-from ${JENKINS_IMAGE} -v $PWD:/data-backup --rm jenkins/ssh-agent:latest \
    bash -c \
    "wget -qO- --no-check-certificate https://raw.githubusercontent.com/sue445/jenkins-backup-script/master/jenkins-backup.sh -O /tmp/jenkins-backup.sh && \
    chmod 755 /tmp/jenkins-backup.sh && \
    /tmp/jenkins-backup.sh ${JENKINS_HOME_PATH} /data-backup/jenkins_$1_backup_`date +%Y%m%d%H%M%S`.tar.gz"
else
  docker run --volumes-from ${JENKINS_IMAGE} -v $PWD:/data-backup --rm jenkins/ssh-agent:latest \
    bash -c "tar cvf /data-backup/jenkins_$1_backup_`date +%Y%m%d%H%M%S.tar.gz` ${JENKINS_HOME_PATH}"
fi

echo "finished."
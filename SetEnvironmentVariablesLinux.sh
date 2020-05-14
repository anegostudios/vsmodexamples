# This script needs a restart and depends on your distribution using PAM. If your linux distribution has a different way to set environment variables, this script will not do anything meaningful.
if [[ -n "${VINTAGE_STORY}" ]]; then
  echo "Environment variable already exists"
elif test -e ~/.pam_environment &&  grep VINTAGE_STORY ~/.pam_environment -q; t$
  echo "Please restart for it to take effect"
else
  #change path to your directory if it is installed somewhere else
  echo "VINTAGE_STORY=~/Desktop/VintageStory" >> ~/.pam_environment
  echo "Environment variable set, please restart for it to take effect"
fi



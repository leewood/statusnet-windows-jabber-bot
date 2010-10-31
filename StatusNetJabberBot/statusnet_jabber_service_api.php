<?php

class CmdChannel
{
    function on($user)
    {
        return false;
    }

    function off($user)
    {
        return false;
    }

    function output($user, $text)
    {
        echo $text;
    }

    function error($user, $text)
    {
        echo $text;
    }

    function source()
    {
        return null;
    }
} 


define('INSTALLDIR', realpath(dirname(__FILE__) . '/..'));

require_once INSTALLDIR.'/scripts/commandline.inc';

require_once INSTALLDIR . '/lib/jabber.php';

$from = $args[0];
$body = $args[1];

$user = User::staticGet('jabber', jabber_normalize_jid($from));

function add_notice(&$user, $from,  $body)
{
    $body = trim($body);
    
    
    $content_shortened = common_shorten_links($body);
    if (Notice::contentTooLong($content_shortened)) {
      $from = jabber_normalize_jid($from);
      $this->from_site($from, sprintf(_('Message too long - maximum is %1$d characters, you sent %2$d.'),
                                      Notice::maxContent(),
                                      mb_strlen($content_shortened)));
      return;
    }

    try {
        $notice = Notice::saveNew($user->id, $content_shortened, 'jabber');
    } catch (Exception $e) {
        echo $content_shortened;
        echo $e->getMessage(); 
        return;
    }

    common_broadcast_notice($notice);
    $notice->free();
    unset($notice);
} 

if ($user != null)
{
  $inter = new CommandInterpreter();
  $cmd = $inter->handle_command($user, $body);
  if ($cmd) {
      $chan = new CmdChannel();
      $cmd->execute($chan);
      
  } else {
      
      add_notice($user, $from, $body);
      
  }


}


?>
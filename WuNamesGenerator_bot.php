<?php
	$body = file_get_contents('php://input');
    $arr = json_decode($body, true);
	include_once('tg.class.php');
	 
	$tg = new tg('*****************tg_api_token*****************'); 
	$tg_id = $arr['message']['chat']['id'];
	 
	$text = $arr['message']['text'];
	
	if($text === "/start") {
	    $tg->send($tg_id, 'Now entah ur full name');
	} else {
    	$json = file_get_contents('*****************url__WuNamesGenerator.php?name=' . urlencode($text));
        $obj = json_decode($json);
        $tg->send($tg_id, '`' . $obj->{'oldname'} . "` from this day forward you will also be known as\r\n`" . $obj->{'newname'} . '`');
	}
	exit('ok');
?>
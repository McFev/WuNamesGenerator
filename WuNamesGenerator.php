<?php
header('Content-Type: application/json; charset=utf-8');

if(!isset($_GET["name"])) {
	$data = array("error" => true, "error_text" => "parameters not set");
	echo json_encode($data);
} else {
	try {
		$name = addslashes($_GET["name"]);
		$array = array(
			'realname'	=> $name,
			'Submit' 	=> 'Enter+the+Wu-Tang'
		);		

		$ch = curl_init('http://www.mess.be/inickgenwuname.php');
		curl_setopt($ch, CURLOPT_POST, 1);
		curl_setopt($ch, CURLOPT_POSTFIELDS, http_build_query($array, '', '&'));
		curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
		curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
		curl_setopt($ch, CURLOPT_HEADER, false);
		$s = curl_exec($ch);
		curl_close($ch);

		$s = substr($s, strpos($s, "<font class=normalText>") + 23);
		$s = substr($s, 0, strpos($s, "</font>"));

		$old = substr($s, 0, strpos($s, "</b>"));

		$new = substr($s, strpos($s, "<font size=2>") + 13);
		$new = substr($new, 0, strpos($new, "</b>"));
		$new = trim(preg_replace('/\s+/m', " ", $new));
		$new = preg_replace('/([\w-]+).* ([\w-]+)/m', "$1 $2", $new);

		$data = array("error" => false, "oldname" => $old, "newname" => $new);
		echo json_encode($data);
	}
	catch (Exception $e) {
		$data = array("error" => true, "error_text" => $e->getMessage());
		echo json_encode($data);
	}
}
?>
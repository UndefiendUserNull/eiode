extends Node

const APP_ID : int = 1371886839408885770

func _ready() -> void:
	DiscordRPC.app_id = APP_ID
	DiscordRPC.large_image = "main"
	DiscordRPC.large_image_text = "Under Development"
	DiscordRPC.start_timestamp = int(Time.get_unix_time_from_system()) 

	
	
	DiscordRPC.refresh()

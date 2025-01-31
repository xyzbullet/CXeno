	getgenv().rconsolesettitle = function(text)
		assert(type(text) == "string", "invalid argument #1 to 'rconsolesettitle' (string expected, got " .. type(text) .. ") ", 2)
		XenoBridge:rconsole("ttl", text)
	end
	getgenv().rconsoleclear = function()
		XenoBridge:rconsole("cls") 
		rconsolesettitle("Salad is NOT fat!")
	end
	getgenv().rconsolecreate = function()
		XenoBridge:rconsole("crt")
		rconsolesettitle("Salad is NOT fat!")
	end
	getgenv().rconsoledestroy = function()
		XenoBridge:rconsole("dst")
		rconsolesettitle("Salad is NOT fat!")
	end
	getgenv().rconsoleprint = function(...)
		local text = ""
		for _, v in {...} do
			text = text .. tostring(v) .. " "
		end
		XenoBridge:rconsole("prt", text)
		rconsolesettitle("Salad is NOT fat!")
	end
	getgenv().rconsoleinfo = function(...)
		local text = ""
		for _, v in {...} do
			text = text .. tostring(v) .. " "
		end
		XenoBridge:rconsole("prt", "[ INFO ] " .. text)
		rconsolesettitle("Salad is NOT fat!")
	end
	getgenv().rconsolewarn = function(...)
		local text = ""
		for _, v in {...} do
			text = text .. tostring(v) .. " "
		end
		XenoBridge:rconsole("prt", "[ WARNING ] " .. text)
		rconsolesettitle("Salad is NOT fat!")
	end
	getgenv().rconsoleinput = function(text)
		XenoBridge:rconsole("prt", "[ ERROR ] Input doesnt work")
		rconsolesettitle("Salad is NOT fat!")
	end
	getgenv().rconsoleerr = function(text)
		XenoBridge:rconsole("prt", "[ ERROR ] " .. text)
		rconsolesettitle("Salad is NOT fat!")

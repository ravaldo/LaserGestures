    #Include %A_ScriptDir%\AHKsock.ahk
    #Include %A_ScriptDir%\Winamp.ahk
    
    Server := "localhost"
    Port   := 5678
    
    ;Register OnExit subroutine so that AHKsock_Close is called before exit
    OnExit, CloseAHKsock
    
    ;Add menu item for exiting gracefully (see comment block in CloseAHKsock)
    Menu, Tray, Add
    Menu, Tray, Add, Exit Gracefully, CloseAHKsock
    
    ;Set up an error handler (this is optional)
    AHKsock_ErrorHandler("AHKsockErrors")
    
    ;Connect to Server on Port
    If (i := AHKsock_Connect(Server, Port, "Recv")) {
        OutputDebug, % "AHKsock_Connect() failed with return value = " i " and ErrorLevel = " ErrorLevel
        ExitApp
    }
Return

CloseAHKsock:
    AHKsock_Close()
ExitApp

Recv(sEvent, iSocket = 0, sName = 0, sAddr = 0, sPort = 0, ByRef bData = 0, iLength = 0) {
    If (sEvent = "CONNECTED") {
        ;Check if the connection attempt was succesful
        If (iSocket = -1) {
            OutputDebug, % "Client - AHKsock_Connect() failed. Exiting..."
            ExitApp
        } Else OutputDebug, % "Client - AHKsock_Connect() successfully connected!"
    } Else If (sEvent = "DISCONNECTED") {
        OutputDebug, % "Client - The server closed the connection. Exiting..."
        ExitApp
    } Else If (sEvent = "RECEIVED") {
	;MsgBox, "Data recieved: " %bData%
        If IsLabel(bData) {
            Gosub %bData%
            return
        }
        If (RegExMatch(bData, "SEEK")) {
            ;MsgBox, "Data recieved: " %bData%
            string := ""
            data := 0
	    RegExMatch(bData, "^.*#SEEK", string)
            RegExMatch(bData, "(?<=SEEK).*", data)
            If IsFunc(string) {
                %string%(data)
                return
            }
        }
    }
}

;We're not actually handling errors here. This is here just to make us aware of errors if any do come up.
AHKsockErrors(iError, iSocket) {
    OutputDebug, % "Client - Error " iError " with error code = " ErrorLevel ((iSocket <> -1) ? " on socket " iSocket : "")
}


;_________________________________________GESTURE OPERATIONS

31:
    Winamp("Play")
return

R:
    Winamp("Next Track")
return

L:
    Winamp("Previous Track")
return

D-D:
    Winamp("Pause")
return

RDL:
    Winamp("Stop")
return

UR:
    Winamp("SetVolume", 255)
return

UL:
    Winamp("SetVolume", 0)
return

R3D1L7U9:
    IfWinExist, ahk_class Winamp v1.x
        Winamp("Exit")
    Else
        run, C:\program files\winamp\winamp.exe ; CHANGE WINAMP PATH HERE
return

U#TURN+:
    Winamp("VolumeUp")
return

U#TURN-:
    Winamp("VolumeDown")
return

D#SEEK(data) {
    Winamp("GetOutputTime", 0)
    pos := ErrorLevel + (data * 20)
    Winamp("JumpToTime", pos)
    return
}
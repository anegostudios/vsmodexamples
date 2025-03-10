package main

import "C"

//export EchoString
func EchoString(input *C.char) *C.char {
	// Convert C string to Go string
	goString := C.GoString(input)

	// Perform some operation if needed
	// For now, just return the input string
	return C.CString(goString)
}

func main() {
	// This is required, but main won't be called
}

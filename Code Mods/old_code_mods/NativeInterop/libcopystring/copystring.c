#include <stdlib.h>
#include <string.h>

// Exported function to copy a string
char* CopyString(const char* input) {
    // Calculate the length of the input string
    size_t len = strlen(input);

    // Allocate memory for a new string (including null terminator)
    char* copy = (char*)malloc(len + 1);

    // Copy the input string to the new memory location
    strcpy(copy, input);

    // Return the copied string
    return copy;
}
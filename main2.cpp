#include <iostream>
#include <string>
#include <cmath>

#define MAX_BITS 100                 		// max number of bits 
#define WIEGAND_WAIT_TIME 300000      		// time to wait for another wiegand pulse.
int binary_decimal(int n);
static unsigned char databits[MAX_BITS];    // stores all of the data bits
static unsigned char bitCount;              // number of bits currently captured
static unsigned int flagDone;               // goes low when data is currently being captured
static unsigned int wiegand_counter;        // countdown until we assume there are no more bits
static unsigned long cardCode=0;            // decoded card code
/* run this program using the console pauser or add your own getch, system("pause") or input loop */
int main() {

	//bitCount++;
	//databits[bitCount] = 100100000001010010100110;
	databits[bitCount] = 0011;
			flagDone = 0;
			wiegand_counter = WIEGAND_WAIT_TIME;  
 
for (int i=0; i<3; i++)
	{
	cardCode <<= 1;
//	cardCode ^= 1;
//	cardCode <<= 1;

//	cardCode |= databits[i];
//	cardCode ^= databits[i];
	}
	cardCode >>= 1;
	printf("\n numero %u", cardCode);
}
/*
int main() {

   int i = 0;
   unsigned int n = 0;
   char bin[] = "100100000001010010100110";

   while ( bin[i] == '0' || bin[i] == '1' ) {
      if ( bin[i] == '0' )
         n <<= 1;
      else {
         n ^= 1;
         n <<= 1;
      }
      ++i;
   }
   n >>= 1;

   printf("%u\n", n);

   return(0);

}*/

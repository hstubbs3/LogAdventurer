#include <stdio.h>
#include <stdlib.h>
#include <sys/stat.h>

/*
TODO 

*/
typedef struct model 
{
	unsigned char predictHighNibble[16][16];
	unsigned char predictHighNibbleLRU[16][16];
	unsigned char predictLowNibble[16][16];
	unsigned char predictLowNibbleLRU[16][16];
} Model;

Model fModel()
{
	Model newb;
	for(int i=0; i<16; i++)
		{
			for(unsigned char j=0;j<15;j++)
				{	
					newb.predictHighNibble[i][j]=(j+1)*16;
					newb.predictHighNibbleLRU[i][j]=j;
					newb.predictLowNibble[i][j]=(j+1)*16;
					newb.predictLowNibbleLRU[i][j]=j;
				}
					newb.predictHighNibble[i][15]=254;
					newb.predictHighNibbleLRU[i][15]=15;
					newb.predictLowNibble[i][15]=255;
					newb.predictLowNibbleLRU[i][15]=15;

		}
	return newb;
}

typedef struct charModelRange
{
	unsigned char highLow;
	unsigned char highHigh;
	unsigned char lowLow;
	unsigned char lowHigh;
} CharModelRange;

int calcScaleUpBy(int low, int high,int by)
{
	return by;
	/*
	if(high - low < 127 - by) return by;
	return 127 - high + low;
	*/
}

int calcScaleUpDouble(int low,int high)
{
	int scaleUp = (high - low +1);
	if((scaleUp << 1) > 128 ) 
		{
			scaleUp = 128 - scaleUp;
			if(scaleUp < 0) scaleUp = 0 ;
		}
	return scaleUp;
}

int calcScaleUpHalf(int low,int high)
{ 
	int scaleUp = (high - low +1) >> 1;
	if((scaleUp << 1) + scaleUp > 127 ) 
		{
			scaleUp = 128 - (scaleUp << 1);
			if(scaleUp < 0) scaleUp = 0 ;
		}
	return scaleUp;
}
CharModelRange getRanges( unsigned char prev,unsigned char input, Model *encodingModel)
{
	int scaleBy = 2;
	int minRangeSize = 3;
	prev >>= 4;
	unsigned char high = input >> 4;
	unsigned char low = input & 15;
	CharModelRange output;
	int freeMe = 0;
	int scaleUp = 0;
	if(high) output.highLow = encodingModel->predictHighNibble[prev][high-1];
	else output.highLow = 0;
	output.highHigh = encodingModel->predictHighNibble[prev][high] - 1;

	if(low) output.lowLow = encodingModel->predictLowNibble[high][low-1];
	else output.lowLow = 0;
	output.lowHigh = encodingModel->predictLowNibble[high][low] - 1;

	// printf("prev: %x high: %x low %x\n",prev,high,low);
	for(int i=0; i<16; i++)
		{
			if(encodingModel->predictHighNibbleLRU[prev][i] == high)
				{
					for(int j = i+1; j<16; j++) encodingModel->predictHighNibbleLRU[prev][j-1] = encodingModel->predictHighNibbleLRU[prev][j];
					encodingModel->predictHighNibbleLRU[prev][15] = high ;
					break;
				}
		}
	for(int i=0; i<16; i++)
		{
			if(encodingModel->predictHighNibbleLRU[high][i] == low)
				{
					for(int j = i+1; j<16; j++) encodingModel->predictHighNibbleLRU[high][j-1] = encodingModel->predictHighNibbleLRU[high][j];
					encodingModel->predictHighNibbleLRU[high][15] = low ;
					break;
				}
		}

	scaleUp = calcScaleUpBy(output.lowLow,output.lowHigh,scaleBy);
	freeMe = encodingModel->predictLowNibble[high][15] + scaleUp - 255;
	if( freeMe > 0) 
		{
			unsigned char LRU;
			// printf("encodingModel->predictLowNibble[%x] - want to scaleUp:%3d but need to reduce Model by: %3d - LRU: ",high,scaleUp,freeMe);
			// for(int i=0; i<16; i++) printf("%x ",encodingModel->predictLowNibbleLRU[high][i]);
			// printf("\nrange ends: ");
			// for(int i=0; i<16; i++) printf("\\%x:%3d  ",i,encodingModel->predictLowNibble[high][i]);
			// printf("\n");
			for(int i = 0; i < 15 ; i++)
				{
					LRU = encodingModel->predictLowNibbleLRU[high][i];
					unsigned char size;
					if(LRU) size = encodingModel->predictLowNibble[high][LRU] - encodingModel->predictLowNibble[high][LRU - 1];
					else size = encodingModel->predictLowNibble[high][LRU] ; 
					if(size >= ( minRangeSize << 1 ) )
						{
							// printf("LRU with sufficient size found at encodingModel->predictLowNibble[%x][%x] size: %3d ",high,LRU,size);
							size = (scaleBy >> 2) + 1;
							if(size > freeMe) size = freeMe;
							// printf(" - reducing by %3d\n",size);
							for(int j = LRU; j<16; j++) encodingModel->predictLowNibble[high][j]-=size;
							// printf("\n");
							freeMe -= size ;
							if(freeMe <= 0) break;
						}
				}
			// printf("range ends: ");
			// for(int i=0; i<16; i++) printf("\\%x:%3d  ",i,encodingModel->predictLowNibble[high][i]);
			// printf("\n");
		}
	scaleUp -= freeMe;
	if(scaleUp)
		{
			for(;low<16;low++) encodingModel->predictLowNibble[high][low] += scaleUp ;
		}

	scaleUp = calcScaleUpBy(output.highLow,output.highHigh,scaleBy);
	freeMe = encodingModel->predictHighNibble[prev][15] + scaleUp - 254;
	if( freeMe > 0) //need to clear space, leave at least 1 for #ENDOFSTREAM !!
		{
			unsigned char LRU;
			// printf("encodingModel->predictHighNibble[%x] - want to scaleUp:%3d but need to reduce Model by %3d, LRU: ",prev,scaleUp,freeMe);
			// for(int i=0; i<16; i++) printf("%x ",encodingModel->predictHighNibbleLRU[prev][i]);
			// printf("\nrange ends: ");
			// for(int i=0; i<16; i++) printf("\\%x:%3d  ",i,encodingModel->predictHighNibble[prev][i]);
			// printf("\n");

			for(int i = 0; i < 15 ; i++)
				{
					LRU = encodingModel->predictHighNibbleLRU[prev][i];
					if( LRU == high) continue ;
					unsigned char size;
					if(LRU) size = encodingModel->predictHighNibble[prev][LRU] - encodingModel->predictHighNibble[prev][LRU - 1];
					else size = encodingModel->predictHighNibble[prev][LRU] ; 
					if(size > ( minRangeSize << 1 ) )
						{
							// printf("reducing using LRU found at encodingModel->predictHighNibble[%x][%x] size: %3d ",prev,LRU,size);
							size = (scaleBy >> 2) + 1;
							if(size > freeMe) size = freeMe;
							// printf(" - reducing by %3d\n",size);
							for(int j = LRU; j<16; j++) encodingModel->predictHighNibble[prev][j]-=size;
							// printf("range ends: ");
							// for(int i=0; i<16; i++) printf("\\%x:%3d ",i,encodingModel->predictHighNibble[prev][i]);
							// printf("\n");
							freeMe -= size ;
							if(freeMe <= 0) break;
						}
				}
		}
	scaleUp -= freeMe;
	if(scaleUp )
		{
			for(;high<16;high++) encodingModel->predictHighNibble[prev][high] += scaleUp ;
		}
	return output;
}

typedef struct 
{
	unsigned char optimal;
	unsigned char numBits;
} doCharBinaryRangeSearchResult ;

doCharBinaryRangeSearchResult doCharBinaryRangeSearch(unsigned char low, unsigned char high)
{
	// printf("doCharBinaryRangeSearch - %3d %3d\n",low,high);
	doCharBinaryRangeSearchResult out;
	unsigned char bitRange = 128;
	unsigned char bitValue = 0;
	for(unsigned char steps = 1; steps < 9; steps++, bitRange/=2)
		{

			// printf("step %d initial range - %3d %3d\n",steps,bitValue,bitValue + bitRange);
			if( bitValue > low) bitValue -= bitRange ;
			if( bitValue < low) bitValue += bitRange ;
			// printf("step %d check range - %3d %3d\n",steps,bitValue,bitValue + bitRange);
			if(bitValue < low || bitValue + bitRange - 1 > high) continue;
			if(bitValue + bitRange - 1 < high) bitValue += bitRange;
			out.optimal = bitValue;
			out.numBits = steps;
			break;
		}
	return out;
}

int doGetOptimalOverlap(unsigned char lowA,unsigned char highA, unsigned char lowB, unsigned char highB)
{
	int bitValue = 0;
	int bitRange = 128;
	if(highA < lowB || highB < lowA){ return -1 ;}

	while(1)
		{
			while((bitValue < lowA) || (bitValue < lowB)) bitValue += bitRange;
			if((bitValue + bitRange <= highA) && (bitValue + bitRange <= highB)) return bitValue;
			bitRange /= 2;
			bitValue -= bitRange;
		}
}

void printTestCompression(FILE *pInFile,FILE *pOutFile,Model *nibblerModel)
{
	//int outputBufferPointer = 0;
	//int outputBufferSize = 1048576;
	//unsigned char *outputBytes = (unsigned char*)malloc(outputBufferSize)
	unsigned char prev = 0;
	unsigned char thisByte;
	CharModelRange theRange;
	doCharBinaryRangeSearchResult optHigh;
	doCharBinaryRangeSearchResult optLow;
	unsigned long outBits = 0;
	unsigned short outByte = 0;
	short goingOut;
	int outBitsUsed = 0;
	int bitsTotal = 0;
	int bytesRead = 0;
	int i;
	int j;
	while(1)
		{
			thisByte = fgetc(pInFile);
			if( feof(pInFile)) break;
			bytesRead++;
			theRange = getRanges(prev,thisByte,nibblerModel);
			optHigh = doCharBinaryRangeSearch(theRange.highLow,theRange.highHigh);
			outBits <<= optHigh.numBits  ;
			outBits += optHigh.optimal >> (8 - optHigh.numBits) ;
			optLow = doCharBinaryRangeSearch(theRange.lowLow,theRange.lowHigh);
			outBits <<= optLow.numBits ;
			outBits += optLow.optimal >> (8 - optLow.numBits) ;
			outBitsUsed += optHigh.numBits + optLow.numBits;
			if( outBitsUsed >= 16) { goingOut = 2; outBitsUsed -= 16; outByte = (outBits >> outBitsUsed ) & 65535; fputc(outByte >> 8,pOutFile); fputc(outByte & 255,pOutFile); outBits &= ( (1 << outBitsUsed) - 1); }
			else if( outBitsUsed >= 8) { goingOut = 1; outBitsUsed -= 8; outByte = (outBits >> outBitsUsed ) & 255; fputc(outByte,pOutFile); outBits &= ( (1 << outBitsUsed) - 1); }
			else goingOut = 0;

			bitsTotal += optHigh.numBits + optLow.numBits;
			/*
			int overlap = doGetOptimalOverlap(theRange.lowLow,theRange.lowHigh,theRange.highLow,theRange.highHigh);
			int overlapBits = 0;

			if(overlap != -1)
				{
					overlapBits = doCharBinaryRangeSearch(overlap,overlap);
					if(overlapBits < bitsHigh + bitsLow)
					{
						bitsTotal += overlapBits - bitsHigh -  bitsLow;
					}
				}
			if( thisByte < 32) printf("\\%02x / %3d - (%3d - %3d),(%3d - %3d) - bits %d %d - %3d %d -> outbytes: %d / %d\n",thisByte,thisByte,theRange.highLow,theRange.highHigh,theRange.lowLow,theRange.lowHigh,bitsHigh,bitsLow,overlap,overlapBits,(bitsTotal +7)/8,bytesRead);

			else printf("'%c' / %3d -  (%3d - %3d),(%3d - %3d) - bits %d %d - %3d %d -> outbytes: %d / %d\n",thisByte,thisByte,theRange.highLow,theRange.highHigh,theRange.lowLow,theRange.lowHigh,bitsHigh,bitsLow,overlap,overlapBits,(bitsTotal +7)/8,bytesRead);
			*/
			if( thisByte < 32 || thisByte > 126) printf("%d : \\%02x",bytesRead,thisByte);
			else printf("%d : '%c'",bytesRead,thisByte);
			printf(" / %3d - %3d %3d|%3d %3d - %3d/%d %3d/%d : %d.%d",thisByte,theRange.highLow,theRange.highHigh,theRange.lowLow,theRange.lowHigh,optHigh.optimal,optHigh.numBits,optLow.optimal,optLow.numBits,(bitsTotal)/8,outBitsUsed);
			if(goingOut) 
				{ 	
					if(goingOut == 1) printf(" :: %02x  ",outByte);
					else printf(" :: %04x",outByte);
				}
			else printf(" ::     ");
			if(outBitsUsed) printf(" : %02lx ",outBits);
			printf("\n");
			prev=thisByte;
		}

	optHigh = doCharBinaryRangeSearch(nibblerModel->predictHighNibble[prev][15],255);
	outBits <<= optHigh.numBits ;
	outBits += optHigh.optimal;
	outBitsUsed += optHigh.numBits;
	bitsTotal += optHigh.numBits;
	printf("%d : END / --- - %3d %3d|--- --- - %3d/%d ---/- : %d.%d :: ",bytesRead+1,nibblerModel->predictHighNibble[prev][15],255,optHigh.optimal,optHigh.numBits,(bitsTotal)/8,outBitsUsed&7);
	while( outBitsUsed > 8 ) { outBitsUsed -= 8; printf("%2lx ",( outBits >> outBitsUsed ) &255);  fputc( (outBits >> outBitsUsed ) &255,pOutFile);}
	printf("%02lx\n\n",(( outBits << (8 - outBitsUsed) ) &255 ) + ( 1 << (8 -outBitsUsed)) -1); 
	fputc((( outBits << (8 - outBitsUsed) ) &255 ) + ( 1 << (8 -outBitsUsed)) -1,pOutFile);

	printf("Final Model used:\n");
	for(prev=0;prev<16;prev++)
		{
			for(unsigned char high=0;high<16;high++)
				{
					if(high) thisByte = nibblerModel->predictHighNibble[prev][high] - nibblerModel->predictHighNibble[prev][high - 1] ;
					else thisByte = nibblerModel->predictHighNibble[prev][high]  ;
					printf("prev nibble \\%x %2d - high nibble \\%x %2d range end: %3d size: %3d\n",prev,prev,high,high,nibblerModel->predictHighNibble[prev][high],thisByte);
				}
		}
	for(prev=0;prev<16;prev++)
		{
			for(unsigned char low=0;low<16;low++)
				{
					if(low) thisByte = nibblerModel->predictLowNibble[prev][low] - nibblerModel->predictLowNibble[prev][low - 1] ;
					else thisByte = nibblerModel->predictLowNibble[prev][low] ;
					unsigned char chara = (prev*16)+low; 
					if(chara < 33 || chara > 126){ chara = 32;}
					printf("high nibble \\%x %2d low nibble \\%x %2d  %c range end: %3d size: %3d\n",prev,prev,low,low,chara, nibblerModel->predictLowNibble[prev][low],thisByte);
				}
		}

	printf("Final Model - condensed form:\n");
	printf("struct { unsigned char predictHighNibble[16][16] = { ");
	for(i = 0; i<16; i++)
		{
			for(j=0; j<16;j++)
			{
				printf("%d , ",nibblerModel->predictHighNibble[i][j]);
			}
		}
	printf(" }; unsigned char predictHighNibbleLRU[16][16] = { ");
	for(i = 0; i<16; i++)
		{
			for(j=0; j<16;j++)
			{
				printf("%d , ",nibblerModel->predictHighNibbleLRU[i][j]);
			}
		}

	printf(" }; unsigned char predictLowNibble[16][16] = { ");
	for(i = 0; i<16; i++)
		{
			for(j=0; j<16;j++)
			{
				printf("%d , ",nibblerModel->predictLowNibble[i][j]);
			}
		}
	printf(" }; unsigned char predictLowNibbleLRU[16][16] = { ");
	for(i = 0; i<16; i++)
		{
			for(j=0; j<16;j++)
			{
				printf("%d , ",nibblerModel->predictLowNibbleLRU[i][j]);
			}
		}
	printf(" }; \n") ;
}

int main(int argc, char **argv)
{
	if(argc < 2)
		{
			printf("Please provide a path for me to try at least... \n");
			return 0;
		}

	printf("\n\n\n\nTesting nibble_compressor.c by encoding/compressing this file here... ummmm...\n%s\n\n\n",argv[1]);
	Model testModel = fModel();
//	for(int i=0; i < 2 ; i ++)
//	{
		char newFile[2048];
		snprintf(newFile,1024,"%s.nc",argv[1]);
		printf("%s\n",newFile);
		FILE *pInFile = fopen(argv[1],"rb");
		FILE *pOutFile = fopen(newFile,"wb+");
//		printf("iteration : %d \n",i);
 		printTestCompression(pInFile,pOutFile,&testModel);
		fclose(pInFile);
		fclose(pOutFile);
//	}
	return 0;
}

using Godot;
using System;
using System.IO;
using System.Text;

public class log_file : Node
{	
	public static class ByteTypes
	{
		public  enum CodePointType : byte
		{
			INVALID,//used for non-word stuff
			BYTEZERO,
			CONTROLCODE,
			WHITESPACE,
			NEWLINE,
			PUNCTUATIONEND,
			DIGITSEPERATOR,
			PUNCTUATIONESCAPE,
			QUOTE,
			BRACKETLEFT,
			BRACKETRIGHT,
			SEPARATOR,
			DIGIT,
			ALPHA,
			CAPITALALPHA,
			LOWERALPHA,
			EXTENDEDCODEPAGE
		}
		public static CodePointType[] char_types = 
			{
				CodePointType.BYTEZERO, //0x00
				CodePointType.CONTROLCODE, //0x01
				CodePointType.CONTROLCODE, //0x02
				CodePointType.CONTROLCODE, //0x03
				CodePointType.CONTROLCODE, //0x04
				CodePointType.CONTROLCODE, //0x05
				CodePointType.CONTROLCODE, //0x06
				CodePointType.CONTROLCODE, //0x07
				CodePointType.CONTROLCODE, //0x08
				CodePointType.WHITESPACE, //0x09 	TAB
				CodePointType.NEWLINE, //0x0A NEWLINE
				CodePointType.CONTROLCODE, //0x0B
				CodePointType.CONTROLCODE, //0x0C
				CodePointType.CONTROLCODE, //0x0D
				CodePointType.CONTROLCODE, //0x0E
				CodePointType.CONTROLCODE, //0x0F
				CodePointType.CONTROLCODE, //0x10
				CodePointType.CONTROLCODE, //0x11
				CodePointType.CONTROLCODE, //0x12
				CodePointType.CONTROLCODE, //0x13
				CodePointType.CONTROLCODE, //0x14
				CodePointType.CONTROLCODE, //0x15
				CodePointType.CONTROLCODE, //0x16
				CodePointType.CONTROLCODE, //0x17
				CodePointType.CONTROLCODE, //0x18
				CodePointType.CONTROLCODE, //0x19
				CodePointType.CONTROLCODE, //0x1A
				CodePointType.CONTROLCODE, //0x1B
				CodePointType.CONTROLCODE, //0x1C
				CodePointType.CONTROLCODE, //0x1D
				CodePointType.CONTROLCODE, //0x1E
				CodePointType.CONTROLCODE, //0x1F
				CodePointType.WHITESPACE,	//0x20 (space)
				CodePointType.PUNCTUATIONEND, //0x21
				CodePointType.PUNCTUATIONEND, //0x22
				CodePointType.SEPARATOR, //0x23
				CodePointType.SEPARATOR, //0x24
				CodePointType.SEPARATOR, //0x25
				CodePointType.SEPARATOR, //0x26
				CodePointType.PUNCTUATIONEND, //0x27
				CodePointType.PUNCTUATIONEND, //0x28
				CodePointType.PUNCTUATIONEND, //0x29
				CodePointType.PUNCTUATIONEND, //0x2A
				CodePointType.SEPARATOR, //0x2B
				CodePointType.DIGITSEPERATOR, //0x2C
				CodePointType.SEPARATOR, //0x2D
				CodePointType.DIGITSEPERATOR, //0x2E
				CodePointType.SEPARATOR, //0x2F
				CodePointType.DIGIT,	//0x300
				CodePointType.DIGIT,	//0x301
				CodePointType.DIGIT,	//0x302
				CodePointType.DIGIT,	//0x303
				CodePointType.DIGIT,	//0x304
				CodePointType.DIGIT,	//0x305
				CodePointType.DIGIT,	//0x306
				CodePointType.DIGIT,	//0x307
				CodePointType.DIGIT,	//0x308
				CodePointType.DIGIT,	//0x309
				CodePointType.PUNCTUATIONEND, //0x3A
				CodePointType.PUNCTUATIONEND, //0x3B
				CodePointType.PUNCTUATIONEND, //0x3C
				CodePointType.PUNCTUATIONEND, //0x3D
				CodePointType.PUNCTUATIONEND, //0x3E
				CodePointType.SEPARATOR, //0x3F
				CodePointType.SEPARATOR, //0x40
				CodePointType.ALPHA, //0x41 A
				CodePointType.ALPHA, //0x42
				CodePointType.ALPHA, //0x43
				CodePointType.ALPHA, //0x44
				CodePointType.ALPHA, //0x45
				CodePointType.ALPHA, //0x46
				CodePointType.ALPHA, //0x47
				CodePointType.ALPHA, //0x48
				CodePointType.ALPHA, //0x49
				CodePointType.ALPHA, //0x4A
				CodePointType.ALPHA, //0x4B
				CodePointType.ALPHA, //0x4C
				CodePointType.ALPHA, //0x4D
				CodePointType.ALPHA, //0x4E
				CodePointType.ALPHA, //0x4F
				CodePointType.ALPHA, //0x50
				CodePointType.ALPHA, //0x51
				CodePointType.ALPHA, //0x52
				CodePointType.ALPHA, //0x53
				CodePointType.ALPHA, //0x54
				CodePointType.ALPHA, //0x55
				CodePointType.ALPHA, //0x56
				CodePointType.ALPHA, //0x57
				CodePointType.ALPHA, //0x58
				CodePointType.ALPHA, //0x59
				CodePointType.ALPHA, //0x5A Z
				CodePointType.BRACKETLEFT, //0x5B
				CodePointType.PUNCTUATIONESCAPE, //0x5C
				CodePointType.BRACKETRIGHT, //0x5D
				CodePointType.PUNCTUATIONEND, //0x5E
				CodePointType.SEPARATOR, //0x5F
				CodePointType.PUNCTUATIONEND, //0x60
				CodePointType.ALPHA,//0x61
				CodePointType.ALPHA,//0x62
				CodePointType.ALPHA,//0x63
				CodePointType.ALPHA,//0x64
				CodePointType.ALPHA,//0x65
				CodePointType.ALPHA,//0x61
				CodePointType.ALPHA,//0x61
				CodePointType.ALPHA,//0x61
				CodePointType.ALPHA,//0x61
				CodePointType.ALPHA,//0x61
				CodePointType.ALPHA,//0x61
				CodePointType.ALPHA,//0x61
				CodePointType.ALPHA,//0x61
				CodePointType.ALPHA,//0x61
				CodePointType.ALPHA,//0x61
				CodePointType.ALPHA,//0x61
				CodePointType.ALPHA,//0x61
				CodePointType.ALPHA,//0x61
				CodePointType.ALPHA,//0x61
				CodePointType.ALPHA,//0x61
				CodePointType.ALPHA,//0x61
				CodePointType.ALPHA,//0x61
				CodePointType.ALPHA,//0x61
				CodePointType.ALPHA,//0x61
				CodePointType.ALPHA,//0x61
				CodePointType.ALPHA,//0x61
				CodePointType.PUNCTUATIONEND, //0x3A
				CodePointType.PUNCTUATIONEND, //0x3A
				CodePointType.PUNCTUATIONEND, //0x3A
				CodePointType.PUNCTUATIONEND, //0x3A
				CodePointType.CONTROLCODE, //0x0B
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE,//0x
				CodePointType.EXTENDEDCODEPAGE//0x				
			};
		public static byte[] is_own_word = //bitfield
			{
				0x00, 0x04, 0x0, 0x0, //0x00 - 0x1F only newline is own word
				0xFE,0xFF,	//0x2x  various punctuation
				0x0,0xFC,	//0x3x	mostly digits, a few punctuation
				0x01,0x0,	//0x4x	mostly upper alpha
				0x0,0xF8,	//0x5x	remaining upper alpha
				0x01,0x0,	//0x6x	lower alpha
				0x00,0x78,	//0x7x,	lower alpha, del is to be considered a control code
				0x0,0x0,0x0,0x0,	//8bits needed / UTF start/continuation, etc... 0x8x-0x9x
				0x0,0x0,0x0,0x0,//0xAx,0xBx
				0x0,0x0,0x0,0x0,//0xCx,0xDx
				0x0,0x0,0x0,0x0//0xEx,0xFx
			};
		public static  CodePointType[] sysLogDateTimeSeconds = 
								{
										ByteTypes.CodePointType.ALPHA,
										ByteTypes.CodePointType.WHITESPACE,
										ByteTypes.CodePointType.DIGIT,
										ByteTypes.CodePointType.WHITESPACE,
										ByteTypes.CodePointType.DIGIT,
										ByteTypes.CodePointType.PUNCTUATIONEND,
										ByteTypes.CodePointType.DIGIT,
										ByteTypes.CodePointType.PUNCTUATIONEND,
										ByteTypes.CodePointType.DIGIT,										
								};
								
		public static bool is_this_own_word(byte char_code)
		{
			return ( is_own_word[char_code >> 3] & ( 1 << (char_code & 7 ) ) ) > 0;
		}
	}
	
	public class LineData
	{
		public byte[] bytes;
		public uint[] bs_word_ends;
		
		//insert constructor here or something..
		//also how to get word by word out?
	}


	public class LineDataPage 
	{
		public LineData[] the_lines;
		public LineDataPage()
		{
			the_lines = new LineData[65536];//~512k on 64 bit?
		}
	}
	
	[Export]
	public uint read_lines_frame = 1;
	[Export]
	public float target_delta = 0.1f;


	string file_name = "\0";
	FileStream the_log_stream;
	bool reading_file = false;
	bool processing_file = false;


//	needed? useful ?
//	public int line_offsets[16777216];//64M to handle up to 16.7M lines
	public LineDataPage[] the_lines_pages ; //likely 512K on a 64bit system.. 64K pages of 64K each = >4Billion...
	// and then filesize * 9/8 if using bitset ( current using 32bit / word end.. so.. mileage varies)
	public int num_lines = 0;

	public word_tree the_words_data;
		
	public void free_all()
	{
		//the_lines_pages = null;
		num_lines = 0;
	}
	public string get_line(int line_num)
	{
		if(num_lines < line_num || line_num < 1)
		{
			return "line number out of range";
		}
		else 
		{
			line_num --;
			LineData the_line = the_lines_pages[line_num >> 16].the_lines[line_num & 65535];
			return Encoding.ASCII.GetString(the_line.bytes,0,the_line.bytes.Length);
		}
	}
	
	public string[] get_line_words(int line_num)
	{
		if(num_lines < line_num || line_num < 1)
		{
			return new string[]{"error"};
		}
		else
		{
			line_num--;
			LineData the_line = the_lines_pages[line_num >> 16].the_lines[line_num & 65535];
			int num_words = 0 ;
			foreach(uint bit_set in the_line.bs_word_ends)
			{
				uint the_bits = bit_set;
				while(the_bits != 0)
				{
					num_words++;
					the_bits &= the_bits - 1 ;
				}
			}
			string[] the_words = new string[num_words];
			int last_end = 0;
			int index = 0;
			num_words = 0;
	//		GD.Print(the_lines[line_num].bytes.Length);
	//		foreach(uint bit_set in the_lines[line_num].bs_word_ends)
	//		{
	//			GD.Print(bit_set);
	//		}
			foreach(uint bit_set in the_line.bs_word_ends)
			{
				for(byte bit = 0; bit < 32; bit++)
				{
					if((bit_set & ((uint) 1 << bit)) > 0)
					{
					//	GD.Print(last_end," - ", index);
						if(index < the_line.bytes.Length)
						{
							the_words[num_words] = Encoding.ASCII.GetString(the_line.bytes,last_end,index - last_end);
					//		GD.Print(the_words[num_words]);
							num_words++;
							last_end = index;
						}
					}
					index++;
				}
			}
			return the_words;
		}
	}
	
	public string get_last_line()
	{
		if(num_lines > 0)
		{
			LineData the_line = the_lines_pages[(num_lines - 1) >> 16].the_lines[(num_lines - 1) & 65535] ;
			return Encoding.ASCII.GetString(the_line.bytes,0,the_line.bytes.Length);
		}
		else 
		{
			return "no log lines entered";
		}
	}
	public int _open_file(string name_of_file)
	{
		if(reading_file){ return -1; } //object is busy
		file_name = name_of_file;
		the_log_stream = System.IO.File.Open(file_name, FileMode.Open);
		if(the_log_stream.CanRead)
		{
			reading_file = true;
			processing_file = true;
			return 0; //success
		}
		else
		{
			return 1; //file not found / unable to open
		}
	}
	
	public bool read_lines_from_file(uint max_lines_to_read)
	{
		long bytes_left = the_log_stream.Length - the_log_stream.Position;
		if( bytes_left <= 0){ return false; }

		byte[] line_buffer = new byte[1048576]; //1M
		uint line_length = 0;
		uint word_length = 0;
		ByteTypes.CodePointType word_type = ByteTypes.CodePointType.INVALID;
		uint[] word_ends = new uint[1048576]; //4M 		5M
		ByteTypes.CodePointType[] word_types = new ByteTypes.CodePointType[1048576]; //1M 		6M
		uint num_words = 0;
		byte next_byte;
		
		while(max_lines_to_read > 0)
		{
			max_lines_to_read--;
			while(bytes_left > 0 && line_length < 1048576)
			{
				bytes_left--;
				next_byte=(byte)the_log_stream.ReadByte();
				line_buffer[line_length] = next_byte;
				line_length++;
				
 				if( ByteTypes.char_types[next_byte] == word_type )
					{
						word_length++;
					}
				else 
					{
						if( word_length > 0 )
							{
								word_ends[num_words] = line_length - 1;
								word_types[num_words] = word_type;
								num_words++;
							}
						if( ByteTypes.is_this_own_word(next_byte) )
						{
							word_type = ByteTypes.CodePointType.INVALID ;
							word_length = 0;
							word_ends[num_words] = line_length;
							word_types[num_words] = ByteTypes.char_types[next_byte] ;
							num_words++;
							if(ByteTypes.char_types[next_byte] == ByteTypes.CodePointType.NEWLINE)
							{
								break;
							}
						}
						else 
						{
							word_type = ByteTypes.char_types[next_byte];
							word_length = 1;
						}
					}
						
			}
			if(word_length > 0)
			{
				word_types[num_words] = word_type;
				word_ends[num_words] = line_length;
				num_words++;
			}
			//add line data to the arrays
			if(line_length > 0)
			{
				if((num_lines & 65535) == 0)
				{
					the_lines_pages[num_lines >> 16] = new LineDataPage();
				}				
				LineData the_line = new LineData();
				the_lines_pages[num_lines >> 16].the_lines[num_lines & 65535] = the_line;

				the_line.bytes = new byte[line_length];
				Array.Copy(line_buffer,the_line.bytes,line_length);
			//	GD.Print(num_lines," : ",Encoding.UTF8.GetString(the_lines[num_lines].bytes,0,(int)line_length));

				the_line.bs_word_ends = new uint[line_length/32 + 1];
				int last_end = 0;
				uint[] new_word_ends = new uint[1048576];
				uint new_num_words = 0 ;
				word_types[num_words] = ByteTypes.CodePointType.INVALID ; 
				num_words++;
				int state = 1; //testing how many words in test file if not creating words from syslog
				for(int i = 0; i < num_words; i++)
				{
					switch(state)
					{
						case 0:
							//starting from start of line.. possibly system date from syslog!
							if(word_ends[9] == 16)
							{
								int k=0;
								foreach(ByteTypes.CodePointType typey in ByteTypes.sysLogDateTimeSeconds )
								{
									if(word_types[k] != typey){ break; }
									k++;
								}
								if(k == 9)
								{
									if(line_buffer[15] == 0x20)
										{
											new_word_ends[0]=15;
											new_word_ends[1]=16;
											i = 10;
										}
									else 
										{
											new_word_ends[0]=word_ends[10];
											new_word_ends[1]=word_ends[11];
											i = 11;
										}
									while(word_types[i] != ByteTypes.CodePointType.WHITESPACE)
									{ i++; }
									new_word_ends[2]=word_ends[i-1];
									new_word_ends[3]=word_ends[i];
									i++;
									while(line_buffer[word_ends[i]-1] != 0x3A &&
										word_types[i] != ByteTypes.CodePointType.BRACKETLEFT)
									{ i++; }
									new_word_ends[4]=word_ends[i-1];
									new_word_ends[5]=word_ends[i]; // : or ]
									if( word_types[i] == ByteTypes.CodePointType.BRACKETLEFT)
									{ 
										i++;
										while(word_types[i] != ByteTypes.CodePointType.BRACKETRIGHT)
										{ i++; }
										new_word_ends[6]=word_ends[i-1];//procID
										new_word_ends[7]=word_ends[i]; //right bracket
										new_word_ends[8]=word_ends[i+1]; //the colon, presumably
										new_word_ends[9]=word_ends[i+2]; //the trailing space
										i += 2;
										new_num_words=10;
									}
									else 
									{
										i++;
										new_word_ends[6]=word_ends[i]; // trailing space
										new_num_words = 7;
									}
									state = 1;
									break;
								}
							}
							// guess not.. fall through to other checks.. 
							state = 1;
							goto case 1;
						case 1:
							switch(word_types[i])
							{
								case ByteTypes.CodePointType.INVALID://beyond end of line, shrink back and end loop
									num_words--;
									break;
						/*
								case ByteTypes.CodePointType.BYTEZERO://a null byte in my wawa ??
									if( i == 0 ) // line starts with NULL ?!?
									{
										new_num_words = 1;
									}
									*/
									
								case ByteTypes.CodePointType.DIGIT: //starting with digit, ok.
									state = 2; 
									break;
								case ByteTypes.CodePointType.ALPHA: //starting with alpha, OK
									state = 3;
									break;
								default:
									new_word_ends[new_num_words] = word_ends[i];
									new_num_words++;
									break;
							}
							break;
						case 2: // starting with digits.
							switch(word_types[i])
							{
								case	 ByteTypes.CodePointType.DIGIT:
								case ByteTypes.CodePointType.DIGITSEPERATOR:
									break;
								case ByteTypes.CodePointType.ALPHA: //OK, could be host name or something..
									state = 3;
									break;
								default:
									i--;
									while(word_types[i] != ByteTypes.CodePointType.DIGIT)
									{ i--;}
									new_word_ends[new_num_words] = word_ends[i];
									new_num_words++;
									state = 1;
									break;
							}
							break;
						case 3: // starting with alpha - could be hostname or something.. glom up..
							switch(word_types[i])
							{
								case	 ByteTypes.CodePointType.DIGIT:
								case ByteTypes.CodePointType.DIGITSEPERATOR:
								case ByteTypes.CodePointType.ALPHA:
								case ByteTypes.CodePointType.SEPARATOR:
									break;
								default:
									i--;
									while(word_types[i] == ByteTypes.CodePointType.DIGITSEPERATOR ||
										word_types[i] == ByteTypes.CodePointType.SEPARATOR)
										{i--;}
									new_word_ends[new_num_words] = word_ends[i];
									new_num_words++;
									state = 1;
									break;
							}
							break;
						default:
							//i--;
						//	state = 1;
							break;
					}
				}

				for(int i=0; i < new_num_words; i++)
				{
					uint word_end = new_word_ends[i];

				//	GD.Print("word ending at : ",word_end);
					the_words_data.update_word_with_line(line_buffer,num_lines,last_end,(int)(word_end-last_end));
					the_line.bs_word_ends[word_end / 32] |= (uint)1 << (byte)(word_end & 31) ;					
					last_end = (int)word_end;
				}
			//	Array.Copy(word_ends,the_lines[num_lines].word_ends,num_words);
				line_length = 0;
				num_words = 0;
				num_lines++;
			}
			if(bytes_left <= 0 ){ return false;}
		}
		return true;
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		the_lines_pages = new LineDataPage[65536];
		the_words_data = new word_tree();
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		if(reading_file)
		{
			if(delta > target_delta) { if(read_lines_frame > 2){ read_lines_frame -= 2; } }
			else { read_lines_frame += 3; }			
			GD.Print(num_lines," - go for another : ",read_lines_frame);
			reading_file = read_lines_from_file(read_lines_frame);
			if(!reading_file)
			{
				the_log_stream.Close();
			}
		}
	}
}

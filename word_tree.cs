using Godot;
using System;
using System.Text;

public class word_tree : Node
{
	
	public class WordData
	{
		public byte[] the_word;
		public int line_count;
		public int first_line; //	as index to lineData, starting with 0!
		public int last_line; // as index to lineData, starting with 0!

		public WordData()
		{
			the_word = new byte[0];
			line_count = 0;
			first_line = -1;
			last_line = -2;
		}
		public WordData(byte[] word_bytes, int line)
		{
			the_word = word_bytes;
			line_count = 1;
			first_line = line;
			last_line = line;
		}
	}// byte[] ref - 8B? so ~20 bytes + length of word at least... OK... 
	
	public class WordDataTreeNodeA
	{
		public  WordDataTreeNodeB[] downs;
		
		public WordDataTreeNodeA()
		{
			downs = new WordDataTreeNodeB[16];
			for(int i=0; i < 16; i++)
			{
				downs[i] = null;
			}
		}
	}//8B per ref? so 16*8 = 128B for this level
	
	public class WordDataTreeNodeB
	{
		public  WordDataTreeNodeA[] downs;
		public  WordData[] ends;
		
		public WordDataTreeNodeB()
		{
			downs = new WordDataTreeNodeA[16];
			ends = new WordData[16];
			for(int i=0; i < 16; i++)
			{
				downs[i] = null;
				ends[i] = null;
			}
		}
	}//8B per ref? so 32*8 = 256B for this level
	/*
	expected memory usage - say only 1 word was in the entire tree... and it is 32 bytes long.
	we have the ROOT node already committed for the tree start.. so the tree overhead is 128B
	the first byte would add a B node, 256B .. the other 31 bytes would each add A,B - 384B
	so 12,160B worth of tree + 20B WordData + 32B bytes array = 12,212B to store that first word.
	But suppose the next word starts with the same byte ( and is also 32 bytes long) - that would take 256B less!
	And the more initial characters shared, the less data would be required.. 
	
	But considering an average worse case - suggesting every word is 16 bytes ascii and expecting ~256K such words..
	there isn't 256K unique start values, only 256 unique byte values, and only 16 unique nibbles..
	we'll consider that the start of each of these words is degenerately as unique as possible...
	first level - ROOT -> 16B //16*128B = 2K, 16 
		0->A	 //16^2*256 = 16*4k = +64k, 256
			1->B //16^3*128 = +512K, 4096
				1->A //16^4*256 = +16M, 64K
					2->B //16^5*128 = +128M, 1M words !!! wait.. not possible, roll back.. 
					2->B //4*16^4*128 = +32M, 256K words ...
					2->A // from here all are assumed unique. so 256+14*384+20+16 per word = ~5K per word
					the rest then - ~1.4GB +... so ~1.5GB for the word data, given ridiculousness
					
					Will just have to see how bad it gets.. 
	*/
	
	private WordDataTreeNodeA ROOT; 
	public int max_line_count;
	public int num_words;
	
	public word_tree()
	{
		ROOT = new WordDataTreeNodeA();
		max_line_count = 1;
		num_words = 0;
	}
	public WordData get_word_data(byte[] word,int start_at=0, int length = 0)
	{
		if(length == 0){ length = word.Length;}
		WordDataTreeNodeA node = ROOT;
		WordDataTreeNodeB next = null;
		byte the_char;
		length--;
		for(int i=0; i < length; i++)
			{
				the_char = word[start_at + i];
				next = node.downs[the_char >> 4];
				if(next == null)
				{
					return null;
				}
				node = next.downs[the_char & 15];
				if(node == null)
				{
					return null;
				}
			}
		the_char = word[start_at + length];
		next = node.downs[the_char >> 4];
		if( next == null)
		{
			return null;
		}
		else 
		{
			return next.ends[the_char & 15];
		}
	}
	
	public void update_word_with_line(byte[] word,int line, int start_at, int length)
	{
		//GD.Print("update_word_with_line called with ",line," : ",start_at," , ", length);
		WordDataTreeNodeA node = ROOT;
		WordDataTreeNodeB next = null;
		
		length--;
		byte the_char;
		for(int i=0; i< length; i++)
			{
				the_char = word[start_at + i];
				//GD.Print(the_char.ToString());
				next = node.downs[the_char >> 4];
				if(next == null)
				{
					next = new WordDataTreeNodeB();
					node.downs[the_char >> 4] = next;
				}
				node = next.downs[the_char & 15];
				if(node == null)
				{
					node = new WordDataTreeNodeA();
					next.downs[the_char & 15] = node;
				}

			}
		the_char = word[start_at + length];
		next = node.downs[the_char >> 4];
		if(next == null)
		{
			next = new WordDataTreeNodeB();
			node.downs[the_char >> 4] = next;
		}
		//GD.Print(the_char.ToString());
		WordData the_word = next.ends[the_char & 15];
		if(the_word == null)
		{
			length++;
			//GD.Print(length);
			//GD.Print(Encoding.ASCII.GetString(word,start_at,length));
			byte[] the_word_bytes = new byte[length];
			Array.Copy(word, start_at,the_word_bytes,0,length); 
			next.ends[the_char & 15 ] = new WordData(the_word_bytes,line);
			num_words += 1 ;
		}
		else
		{
			if(the_word.last_line < line)
			{
				the_word.line_count += 1;
				the_word.last_line = line;
				
				if(the_word.line_count > max_line_count && the_word.line_count < ( line >> 4 ) )
				{
					max_line_count= the_word.line_count;
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Managed.X86;
using System.IO;

namespace Demo {
	class Program {
		static void Main(string[] args) {
			byte[] bytes;

			using (MemoryStream output = new MemoryStream()) {
				X86Writer writer = new X86Writer(output, new IntPtr(0x00400A00));

				//00    53              PUSH EBX
				//01    52              PUSH EDX
				//02    57              PUSH EDI
				//03    56              PUSH ESI
				//04    8B5C24 10       MOV EBX,DWORD PTR SS:[ESP+10]
				//08    8B30            MOV ESI,DWORD PTR DS:[EAX]
				//0A    BF ********     MOV EDI,[DATA CAVE]
				//0F  > 66:8B16         MOV DX,WORD PTR DS:[ESI]
				//12  | 66:8917         MOV WORD PTR DS:[EDI],DX
				//15  | 83C6 02         ADD ESI,2
				//18  | 83C7 02         ADD EDI,2
				//1B  | 66:83FA 00      CMP DX,0
				//1F  ^ 75 EE           JNZ SHORT 0F
				//21    5E              POP ESI
				//22    5F              POP EDI
				//23    8B5C24 0C       MOV EBX,DWORD PTR SS:[ESP+C]
				//27    8B5424 08       MOV EDX,DWORD PTR SS:[ESP+8]
				//2B    8BCD            MOV ECX,EBP
				//2D    887C24 64       MOV BYTE PTR SS:[ESP+64],BH
				//31    895424 0C       MOV DWORD PTR SS:[ESP+C],EDX
				//35    5A              POP EDX
				//36    5B              POP EBX
				//37    83C4 04         ADD ESP,4
				//3A    C3              RETN

				uint cave = 0xCD004000;

				X86Register32 eax = X86Register32.EAX;
				X86Register32 ebx = X86Register32.EBX;
				X86Register32 ecx = X86Register32.ECX;
				X86Register32 edx = X86Register32.EDX;
				X86Register32 ebp = X86Register32.EBP;
				X86Register32 esp = X86Register32.ESP;
				X86Register32 esi = X86Register32.ESI;
				X86Register32 edi = X86Register32.EDI;

				X86Register16 dx = X86Register16.DX;
				X86Register8 bh = X86Register8.BH;

				writer.CreateLabel("start").Mark();
				writer.Push32(ebx);
				writer.Push32(edx);
				writer.Push32(edi);
				writer.Push32(esi);
				writer.Mov32(ebx, new X86Address(esp, 0x10));
				writer.Mov32(esi, new X86Address(eax, 0x00));
				writer.Mov32_(edi, cave);

				writer.CreateLabel("LoopStart").Mark();
				writer.Mov16(dx, new X86Address(esi, 0x00));
				writer.Mov16(new X86Address(edi, 0x00), dx);
				writer.Add32_(esi, 0x02);
				writer.Add32_(edi, 0x02);
				writer.Cmp16_(dx, 0x00);
				writer.Jmp(X86ConditionCode.NotZero, "LoopStart");

				writer.Pop32(esi);
				writer.Pop32(edi);
				writer.Mov32(ebx, new X86Address(esp, 0x0c));
				writer.Mov32(edx, new X86Address(esp, 0x08));
				writer.Mov32(ecx, ebp);
				writer.Mov8(new X86Address(esp, 0x64), bh);
				writer.Mov32(new X86Address(esp, 0x0C), edx);
				writer.Pop32(edx);
				writer.Pop32(ebx);
				writer.Add32_(esp, 0x04);

				writer.Retn();

				writer.Nop(); writer.Nop(); writer.Nop(); writer.Nop(); writer.Nop();

				writer.Push32(0x06);
				writer.Call("start");

				bytes = output.ToArray();
			}

			for (int i = 0; i < bytes.Length; i++) {
				if (i % 8 == 0) {
					string adr = i.ToString("X");
					if (adr.Length == 1) adr = "0" + adr;
					Console.Write(adr + ": ");
				}

				string bt = bytes[i].ToString("X");
				if (bt.Length == 1) bt = "0" + bt;
				Console.Write(bt + " ");

				if ((i + 1) % 8 == 0) Console.WriteLine();
			}

			Console.WriteLine();
			Console.ReadLine();
		}
	}
}

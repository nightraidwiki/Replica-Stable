using System;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using Replica.Engine.Interop;

namespace Replica.Engine.Util;

internal static class ChatSender
{
	public unsafe static void Send(string message)
	{
		if (string.IsNullOrWhiteSpace(message))
		{
			return;
		}
		try
		{
			UIModule* ptr = UIModule.Instance();
			if (ptr == null)
			{
				return;
			}
			Utf8String utf8String = new Utf8String();
			utf8String.Ctor();
			try
			{
				utf8String.SetString(message);
				utf8String.SanitizeString(AllowedEntities.UppercaseLetters | AllowedEntities.LowercaseLetters | AllowedEntities.Numbers | AllowedEntities.SpecialCharacters | AllowedEntities.CharacterList | AllowedEntities.OtherCharacters | AllowedEntities.Payloads | AllowedEntities.Unknown9, null);
				if (utf8String.Length != 0)
				{
					ptr->ProcessChatBoxEntry(&utf8String, (nint)(&utf8String));
				}
			}
			finally
			{
				utf8String.Dtor();
			}
		}
		catch (Exception exception)
		{
			Svc.Log?.Error(exception, "[Replica] chat send failed");
		}
	}
}

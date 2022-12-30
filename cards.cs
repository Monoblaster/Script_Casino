function Cards_Create()
{
	return new ScriptObject(){class = "Cards";};
}

function Cards::Set(%c,%n,%cards)
{
	if(%c.cNum[%n] $= "")
	{
		%c.cName[%c.cCount + 0] = %n;
		%c.cNum[%n] = %c.cCount + 0;
		%c.cCount++;
	}
	
	%c.cCards[%n] = %cards;
	return %c;
}

function Cards::Get(%c,%n)
{	
	return %c.cCards[%n];
}

function Cards::Clear(%c,%cards)
{
	%count = %c.cCount;
	for(%i = 0; %i < %count; %i++)
	{
		%c.cCards[%c.cName[%i]] = "";
		%c.cNum[%c.cName[%i]] = "";
		%c.cName[%i] = "";
	}
	%c.cCount = 0;
	
	return %c;
}

function Cards::Shuffle(%c,%n)
{
	%cards = %c.get(%n);
	%count = getWordCount(%cards);
	for(%i = 0; %i < %count; %i++)
	{
		%r = getRandom(%count - 1);
		%cards = setWord(setWord(%cards,%i,getWord(%cards,%r)),%r,getWord(%cards,%i));
	}
	
	%c.set(%n,%cards);
	return %c;
}

function Cards::Move(%c,%from,%a,%to,%b)
{
	%fromcards = %c.get(%from);
	%tocards = %c.get(%to);
	
	%c.set(%to,setWord(%tocards,%b,trim(getWord(%tocards,%b) SPC getWord(%fromcards,%a))));
	%c.set(%from,removeWord(%fromcards,%a));
	return %c;
}
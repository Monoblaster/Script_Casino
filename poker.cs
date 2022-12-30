$c = -1;
$Poker::Type[$c++] = "High Card";
$Poker::Type[$c++] = "One Pair";
$Poker::Type[$c++] = "Two Pair";
$Poker::Type[$c++] = "Three of a Kind";
$Poker::Type[$c++] = "Straight";
$Poker::Type[$c++] = "Flush";
$Poker::Type[$c++] = "Full House";
$Poker::Type[$c++] = "Four of a Kind";
$Poker::Type[$c++] = "Straight Flush";

$c = -1;
$Poker::Type[$Poker::Type[$c]] = $c++;
$Poker::Type[$Poker::Type[$c]] = $c++;
$Poker::Type[$Poker::Type[$c]] = $c++;
$Poker::Type[$Poker::Type[$c]] = $c++;
$Poker::Type[$Poker::Type[$c]] = $c++;
$Poker::Type[$Poker::Type[$c]] = $c++;
$Poker::Type[$Poker::Type[$c]] = $c++;
$Poker::Type[$Poker::Type[$c]] = $c++;
$Poker::Type[$Poker::Type[$c]] = $c++;
function Poker_Type(%s)
{
	return $Poker::Type[%s];
}

$c = -1;
$Poker::Card[$c++] = "Ace";
$Poker::Card[$c++] = "Two";
$Poker::Card[$c++] = "Three";
$Poker::Card[$c++] = "Four";
$Poker::Card[$c++] = "Five";
$Poker::Card[$c++] = "Six";
$Poker::Card[$c++] = "Seven";
$Poker::Card[$c++] = "Eight";
$Poker::Card[$c++] = "Nine";
$Poker::Card[$c++] = "Ten";
$Poker::Card[$c++] = "Jack";
$Poker::Card[$c++] = "Queen";
$Poker::Card[$c++] = "King";

$c = -1;
$Poker::Card[$Poker::Card[$c]] = $c++;
$Poker::Card[$Poker::Card[$c]] = $c++;
$Poker::Card[$Poker::Card[$c]] = $c++;
$Poker::Card[$Poker::Card[$c]] = $c++;
$Poker::Card[$Poker::Card[$c]] = $c++;
$Poker::Card[$Poker::Card[$c]] = $c++;
$Poker::Card[$Poker::Card[$c]] = $c++;
$Poker::Card[$Poker::Card[$c]] = $c++;
$Poker::Card[$Poker::Card[$c]] = $c++;
$Poker::Card[$Poker::Card[$c]] = $c++;
$Poker::Card[$Poker::Card[$c]] = $c++;
$Poker::Card[$Poker::Card[$c]] = $c++;
$Poker::Card[$Poker::Card[$c]] = $c++;

$Poker::Suit[1] = "Clubs";
$Poker::Suit[2] = "Hearts";
$Poker::Suit[4] = "Spades";
$Poker::Suit[8] = "Diamonds";

$Poker::Suit["Clubs"] = 1;
$Poker::Suit["Hearts"] = 2;
$Poker::Suit["Spades"] = 4;
$Poker::Suit["Diamonds"] = 8;

function Poker_Compare(%evalA,%evalB)
{
	%typeA = getWord(%evalA,0);
	%typeB = getWord(%evalB,0);
	
	%winner = -1;
	if(%typeA > %typeB)
	{
		%winner = 1;
	}

	if(%evalA == %evalB)
	{
		%sortedA = getWords(%evalA,1);
		%sortedB = getWords(%evalB,1);
		%winner = strcmp(%sortedA, %sortedB);
	}

	return %winner;
}

function Poker_Rank(%n)
{
	return %n % 13;
}

function Poker_Suit(%n)
{
	return 1 << mFloor(%n / 13);
}

function Poker_Name(%n)
{
	return $Poker::Card[%n % 13] SPC "of" SPC $Poker::Suit[1 << mFloor(%n / 13)];
}

function Poker_Num(%s)
{
	echo($Poker::Card[getWord(%s,0)] + mLog($Poker::Suit[getWord(%s,2)])/mLog(2) * 13);
}

function Poker_Bitmap(%n)
{
	%rank = %n % 13 + 1;
	if(%rank < 10)
	{
		%rank = 0 @ %rank;
	}
	return "<bitmap:add-ons/script_casino/c" @ getSubStr($Poker::Suit[1 << mFloor(%n / 13)],0,1) @ %rank @ ">";
}
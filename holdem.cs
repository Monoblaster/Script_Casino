$Deck::Default = "";
for($i = 0; $i < 52; $i++)
{
	$Deck::Default = $Deck::Default SPC $i;
}
$Deck::Default = trim($Deck::Default);

function Holdem_Create()
{
	return new ScriptObject(){class = "Holdem";};
}

function Holdem::OnAdd(%obj)
{
	%obj.cards = Cards_Create();
	%obj.seats = Seats_Create(19);
}

function Holdem::OnRemove(%obj)
{
	%obj.cards.delete();
	%obj.seats.delete();
}

function Holdem::setPlayer(%obj,%n,%in,%stack)
{
	%obj.seats.setIn(%n,%in);
	%obj.playerStack[%n] = %stack;

	if(!%in && %obj.playerFolded[%n])
	{
		%obj.foldedCount--;
	}

	return %obj;
}

function Holdem::addPlayer(%obj,%stack,%seat)
{
	if(%seat $= "")
	{
		%seat = %obj.seats.next(0,true);
	}
	
	%obj.setPlayer(%seat,true,%stack);
	%obj.playerFolded[%seat] = true;
	%obj.foldedCount++;
	return %seat;
}

function Holdem::playerStack(%obj,%n)
{
	return %obj.playerStack[%n];
}

function Holdem::playerStake(%obj,%n)
{
	return %obj.playerStake[%n];
}

function Holdem::playerIn(%obj,%n)
{
	return %obj.seats.in(%n);
}

function Holdem::PlayerFolded(%obj,%n)
{
	return %obj.playerFolded[%n];
}

%c = -1;
$Holdem::Round[%c++] = "Pre-Flop";
$Holdem::Round[%c++] = "Flop";
$Holdem::Round[%c++] = "Turn";
$Holdem::Round[%c++] = "River";
function Holdem::roundName(%obj)
{
	return $Holdem::Round[%obj.round];
}

function Holdem::round(%obj)
{
	return %obj.round;
}

function Holdem::curr(%obj)
{
	return %obj.seats.curr();
}

function Holdem::currBet(%obj)
{
	return %Obj.lastBet;
}

function Holdem::MinStake(%obj)
{
	return %Obj.minStake;
}

function Holdem::over(%obj)
{
	return (%obj.seats.count() - %obj.foldedCount <= 1) || %obj.round == 4 || (%obj.seats.count() - %obj.foldedCount - %obj.allInCount == 1 && %obj.noChangeCount >= 1) 
	|| %obj.seats.count() - %obj.foldedCount - %obj.allInCount == 0;
}

function Holdem::passed(%obj)
{
	return %obj.noChangeCount >= (%obj.seats.count() - %obj.foldedCount - %obj.allInCount);
}

function Holdem::reveal(%obj)
{
	return (%obj.seats.count() - %obj.foldedCount > 1);
}

function Holdem::NextHand(%obj,%bigBlind)
{
	%seats = %obj.seats;
	%playerCount = %seats.count();
	if(%playerCount < 2)
	{
		return false;
	}

	%count = %obj.potCount;
	for(%i = 0; %i < %count; %i++)
	{
		%obj.pot[%i] = 0;
	}

	//reset
	%obj.allInCount = 0;
	%obj.foldedCount = 0;
	%obj.noRaiseCount = 0;
	%obj.potCount = 1;
	%obj.lastBet = 0;
	%obj.minStake = 0;
	%obj.newPot = false;

	for(%i = 0; %i < 19; %i++)
	{
		%obj.playerFolded[%i] = false;
		%obj.playerAllIn[%i] = false;
		%obj.eligablePot[%i] = 0;
		%obj.playerStake[%i] = 0;
	}

	//set the dealer button
	%obj.dealerbutton = %dealerButton = %seats.next(%obj.dealerButton + 1);

	if(%playerCount == 2)
	{
		//head to head
		%seats.setCurr(%dealerButton);
	}
	else
	{
		%seats.setCurr(%dealerButton + 1);
	}
	
	%blind = mFloor(%bigBlind / 2);
	%curr = %seats.curr();
	if(%obj.playerStack[%curr] > %blind)
	{
		%obj.lastBet = %blind;
		%obj.noChangeCount = 1;
		%obj.addToPot(%blind);
		%obj.nextTurn();
	}
	else
	{
		%obj.allIn().nextTurn();
	}

	%blind = mFloor(%bigBlind);
	%curr = %seats.curr();
	if(%obj.playerStack[%curr] > %blind)
	{
		%obj.lastBet = %blind;
		%obj.noChangeCount = 1;
		%obj.addToPot(%blind);
		%obj.nextTurn();
	}
	else
	{
		%obj.allIn().nextTurn();
	}
	
	//reset deck
	%cards = %obj.cards.clear().set("deck",$Deck::Default).shuffle("deck");
	
	//deal
	%player = %seats.next(%dealerButton + 1);
	for(%i = 0; %i < %playerCount * 2; %i++)
	{
		%cards.move("deck",0,"hand" @ %player,0);
		%player = %seats.next(%player + 1);
	}

	%obj.round = 0;
	%obj.noChangeCount = 0;

	return true;
}

function Holdem::nextTurn(%obj)
{
 	%curr = %obj.seats.afterCurr();
	%safety = 20;
	while(%obj.playerFolded[%curr] || %obj.playerAllIn[%curr])
	{
		%curr = %obj.seats.afterCurr();
		if(%safety-- == 0)
		{
			talk("ERROR: Holdem::nextTurn() safety broken");
			break;
		}
	}
	return %obj;
}

function Holdem::nextRound(%obj)
{
	%obj.noChangeCount = 0;
	%obj.lastBet = 0;
	%curr = %obj.seats.setCurr(%obj.dealerButton + 1);
	%safety = 20;
	while(%obj.playerFolded[%curr] || %obj.playerAllIn[%curr])
	{
		%curr = %obj.seats.afterCurr();
		if(%safety-- == 0)
		{
			talk("ERROR: Holdem::nextRound() safety broken");
			break;
		}
	}
	%obj.round++;
	return %obj;
}

function Holdem::dealCommunity(%obj)
{
	%obj.cards.move("deck",0,"community",getWordCount(%obj.cards.get("community")));
	return %obj;
}

function Holdem::canAllIn(%obj)
{
	return %obj.seats.count() - %obj.foldedCount - %obj.allInCount != 1 || %obj.playerStack[%obj.seats.curr()] < %obj.lastBet;
}

function Holdem::canRaise(%obj)
{
	return %obj.playerStack[%obj.seats.curr()] > %obj.lastBet && %obj.seats.count() - %obj.foldedCount - %obj.allInCount != 1;
}

function Holdem::canCall(%obj)
{
	return %obj.playerStack[%obj.seats.curr()] >= %obj.lastBet && %obj.lastBet != 0 && %obj.playerStake[%obj.seats.curr()] != %obj.minStake;
}

function Holdem::canCheck(%obj)
{
	return %obj.playerStake[%obj.seats.curr()] == %obj.minStake;
}

//removed cause it doesn't make any sense in the context of texas holdem
//the round is already opened by blinds so betting shouldn't be an option
function Holdem::canBet(%obj)
{
	return false;
	// return %obj.lastBet == 0 && %obj.playerStack[%obj.seats.curr()] > 0 && %obj.seats.count() - %obj.foldedCount - %obj.allInCount != 1;
}

function Holdem::fold(%obj)
{
	%obj.playerFolded[%obj.seats.curr()] = true;
	%obj.foldedCount++;
	return %obj;
}

function Holdem::check(%obj)
{
	%obj.noChangeCount++;
	return %obj;
}

function Holdem::allIn(%obj)
{
	%curr = %obj.seats.curr();
	if(%obj.newPot)
	{
		%newPotValue = 0;
		%possiblestake = %obj.playerStack[%curr] + %obj.playerStake[%curr];
		if(%possiblestake < %obj.minStake())
		{
			%newPotValue = %obj.pot[%obj.potCount - 1] - (%obj.minStake - %possiblestake);
			%obj.pot[%obj.potCount - 1] = %obj.minStake - %possiblestake; // set old pot to this new value so new bets don't effect it
		}
		%obj.pot[%obj.potCount++ - 1] = %newPotValue;
		
		%obj.newPot = false;
	}

	%obj.allInCount++;
	%obj.playerAllIn[%obj.seats.curr()] = true;

	%obj.call();
	
	%stack = %obj.playerStack[%obj.seats.curr()];
	if(%stack > 0)
	{
		%obj.bet(%stack);
		%obj.noChangeCount = 0;
	}
	%obj.newPot = true;
	return %obj;
}

function Holdem::raise(%obj,%n)
{
	if(%obj.playerStack[%obj.seats.curr()] >= (%n + %obj.minStake() - %obj.playerStack[%obj.seats.curr()]))
	{
		%obj.call();
		%obj.bet(%n);
	}
	return %obj;
}

function Holdem::call(%obj)
{
	%curr = %obj.seats.curr();
	%obj.addToPot(getMin(%obj.playerStack[%curr],%obj.minStake - %obj.playerStake[%curr]));
	%obj.noChangeCount++;
	return %obj;
}

function Holdem::bet(%obj,%n)
{
	if(%obj.newPot)
	{
		%obj.pot[%obj.potCount++ - 1] = 0;
		%obj.newPot = false;
	}
	%obj.lastBet = %n;
	%obj.noChangeCount = 1;
	%obj.addToPot(%n);
	
	return %obj;
}

function Holdem::addToPot(%obj,%n)
{
	%curr = %obj.seats.curr();
	%obj.playerStack[%curr] -= %n;
	%obj.pot[%obj.potCount - 1] += %n;
	%obj.eligablePot[%curr] = %obj.potCount - 1;
	%obj.playerStake[%curr] += %n;
	%obj.minStake = %obj.playerStake[%curr];
	return %obj;
}

$c = -1;
$Holdem::RankA[$c++] = "n";
$Holdem::RankA[$c++] = "a";
$Holdem::RankA[$c++] = "b";
$Holdem::RankA[$c++] = "c";
$Holdem::RankA[$c++] = "d";
$Holdem::RankA[$c++] = "e";
$Holdem::RankA[$c++] = "f";
$Holdem::RankA[$c++] = "g";
$Holdem::RankA[$c++] = "h";
$Holdem::RankA[$c++] = "i";
$Holdem::RankA[$c++] = "j";
$Holdem::RankA[$c++] = "k";
$Holdem::RankA[$c++] = "l";
$Holdem::RankA[$c++] = "m";

function Holdem_RankA(%n)
{
	return $Holdem::RankA[%n % 13];
}

function Holdem_Eval(%hand)
{
	%cardCount = getWordCount(%hand);

	for(%i = 0; %i < %cardCount; %i++)
	{
		%n = getWord(%hand,%i);
		%rank = Poker_Rank(%n);
		%suit = Poker_Suit(%n);

		//histogram of the ranks
		%words = getField(%rankHistogram,%rank);
		%rankHistogram = setField(%rankHistogram,%rank,getWord(%words,0) + 1 SPC %rank);
		//and of all the suits (should be a value left over if they're all the same suits)
		%words = getField(%suitHistogram,%suit);
		%suitHistogram = setField(%suitHistogram,%suit,getWord(%words,0) + 1 SPC %suit);
	}
	//sort the histogram values from highest to lowest with rank breaking ties
	%rankHistogram = trim(sortFields(%rankHistogram,"getWord(%v1,0) > getWord(%v2,0) || ((getWord(%v1,1) > getWord(%v2,1) && getWord(%v2,1) != 0)"
	@ " || getWord(%v1,1) == 0 && getWord(%v2,1) != 0) && getWord(%v1,0) == getWord(%v2,0)"));
	%suitHistogram = trim(sortFields(%suitHistogram,"getWord(%v1,0) > getWord(%v2,0)"));

	%count = getFieldCount(%rankHistogram);
	%sameRanks = "";
	%rankComparison = "";
	for(%i = 0; %i < %count; %i++)
	{
		%words = getField(%rankHistogram,%i);
		%sameRanks = %sameRanks @ getWord(%words,0);
		%rankComparison = %rankComparison @ Holdem_RankA(getWord(%words,1));
	}
	%bestSuit = getWord(getField(%suitHistogram,0),1); 
	%sortedHand = sortWords(%hand,"Poker_Suit(%v1) == " @ %bestSuit @ " && Poker_Suit(%v2) == " @ %bestSuit @ "&& Poker_Rank(%v1) > Poker_Rank(%v2) || Poker_Suit(%v1) == " @ %bestSuit @ " &&  Poker_Suit(%v2) != " @ %bestSuit);
	for(%i = 0; %i < %cardCount; %i++)
	{
		%flushcomparison = %flushcomparison @ Holdem_RankA(getWord(%sortedHand,%i));
	}

	//sort hand by value to check for straight and flush
	%sortedHand = sortWords(%hand,"Poker_Rank(%v1) > Poker_Rank(%v2)");
	for(%i = 0; %i < %cardCount; %i++)
	{
		%comparison = %comparison @ Holdem_RankA(getWord(%sortedHand,%i));
	}

	%count = %cardCount - 4;
	for(%i = 0; %i < %count; %i++)
	{
		%straight = true;
		for(%j = 0; %j < 4; %j++)
		{
			if(Poker_Rank(getWord(%sortedHand,%i + %j)) - Poker_Rank(getWord(%sortedHand,%i + %j + 1)) == 1)
			{
				continue;
			}

			%straight = false;
			break;
		}

		%straight[%i] = %straight;
		if(%straight)
		{
			%flush = true;
			for(%j = 0; %j < 5; %j++)
			{
				if(Poker_Suit(getWord(%sortedHand,%i + %j)) == %bestSuit)
				{
					continue;
				}

				%flush = false;
				break;
			}

			if(%flush)
			{
				return Poker_Type("Straight Flush") SPC %flushcomparison;
			}
		}
	}

	if(getSubStr(%sameRanks,0,1) == 4)
	{
		return Poker_Type("Four of a Kind") SPC %rankComparison;
	}
	else if(getWord(%suitHistogram,0) >= 5)
	{
		return Poker_Type("Flush") SPC %flushcomparison;
	}

	for(%i = 0; %i < %count; %i++)
	{
		if(%straight[%i])
		{
			return Poker_Type("Straight") SPC %comparison;
		}
	}

	if(getSubStr(%sameRanks,0,2) == 32)
	{
		return Poker_Type("Full House") SPC getSubStr(%rankComparison,0,2);
	}
	else if(getSubStr(%sameRanks,0,1) == 3)
	{
		return Poker_Type("Three of a Kind") SPC getSubStr(%rankComparison,0,3);
	}
	else if(getSubStr(%sameRanks,0,2) == 22)
	{
		return Poker_Type("Two Pair") SPC getSubStr(%rankComparison,0,2);
	}
	else if(getSubStr(%sameRanks,0,1) == 2)
	{
		return Poker_Type("One Pair") SPC getSubStr(%rankComparison,0,4);
	}

	return Poker_Type("High Card") SPC %comparison;
}

function Holdem::showdown(%obj)
{
	%cards = %obj.cards;
	%seats = %obj.seats;

	%community = %cards.get("community");

	%count = %seats.count();
	%player = %seats.next(0);
	for(%i = 0; %i < %count; %i++)
	{
		if(!%obj.playerFolded[%player])
		{
			%compare = Holdem_Eval(%community SPC %cards.get("hand" @ %player));
			%compare[%player] = setWord(%compare,1,getSubStr(getWord(%compare,1),0,5));
		}
		%obj.playerStake[%player] = 0;
		%player = %seats.next(%player + 1);
	}

	%seats = %obj.seats;
	for(%potDepth = 0; %potDepth < %obj.potCount; %potDepth++)
	{
		%player = %seats.next(%obj.dealerButton + 1);
		%winners = "";
		%winnerCompare = "";
		for(%i = 0; %i < %count; %i++)
		{
			
			if(%obj.eligablePot[%player] <= %potDepth)
			{
				%currCompare = %compare[%player];
				%diff = Poker_Compare(%currCompare,%winnerCompare);
				if(%diff == 1)
				{
					%winners = %player;
					%winnerCompare = %currCompare;
				}
				else if(%diff == 0)
				{
					%winners = %winners SPC %player;
				}
			}
			%player = %seats.next(%player + 1);
		}

		%winnerCount = getWordCount(%winners);
		%split = %obj.pot[%potDepth] / %winnerCount;
		%obj.playerStack[getWord(%winners,0)] += mCeil(%split);
		for(%i = 1; %i < %winnerCount; %i++)
		{
			%obj.playerStack[getWord(%winners,%i)] += mFloor(%split);
		}
		%output = %output TAB %potDepth SPC getWord(%winnerCompare,0) SPC %obj.pot[%potDepth] SPC %winners;
	}

	return trim(%output);
}	
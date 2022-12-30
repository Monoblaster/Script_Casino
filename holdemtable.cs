datablock PlayerData(CasinoCardPlayer)
{
    shapeFile = "add-ons/Item_PlayingCards/tex/cards.dts";
	boundingBox = vectorScale("20 20 20", 4);
	crouchBoundingBox = vectorScale("20 20 20", 4);

	splash = PlayerSplash;
    splashEmitter[0] = PlayerFoamDropletsEmitter;
    splashEmitter[1] = PlayerFoamEmitter;
    splashEmitter[2] = PlayerBubbleEmitter;

    mediumSplashSoundVelocity = 10;
    hardSplashSoundVelocity = 20;
    exitSplashSoundVelocity = 5;

    impactWaterEasy = Splash1Sound;
    impactWaterMedium = Splash1Sound;
    impactWaterHard = Splash1Sound;
    exitingWater = exitWaterSound;

    jetEmitter = playerJetEmitter;
    jetGroundEmitter = playerJetGroundEmitter;
    jetGroundDistance = 4;
    footPuffNumParts = 10;
    footPuffRadius = 0.25;
};

function CasinoCardPlayer::Create(%data)
{
	return new Player(){dataBlock = %data;};
}

function CasinoCardPlayer::OnAdd(%data,%obj)
{
	%obj.applyDamage(1000);
	%obj.hideNode("ALL");
	%obj.setNodeColor("ALL","1 1 1 1");
}

function CasinoCardPlayer::SetCard(%data,%obj,%n)
{
	switch (mFloor(%n / 13)) {
		case 0: %suit = "c_";
		case 1: %suit = "h_";
		case 2: %suit = "s_";
		case 3: %suit = "d_";
	}
	%s = %suit @ (%n % 13 + 1);

	%obj.hideNode("ALL");
	if(%n !$= "")
	{
		%obj.unHideNode("cardback");
		%obj.unHideNode("card");
		%obj.unHideNode(%s);
	}
	return %obj;
}

datablock PlayerData(CasinoCardHolderPlayer) {
	shapeFile = "add-ons/Item_PlayingCards/tex/cardHolder.dts";

	boundingBox = vectorScale("20 20 20", 4);
	crouchBoundingBox = vectorScale("20 20 20", 4);

	splash = PlayerSplash;
    splashEmitter[0] = PlayerFoamDropletsEmitter;
    splashEmitter[1] = PlayerFoamEmitter;
    splashEmitter[2] = PlayerBubbleEmitter;

    mediumSplashSoundVelocity = 10;
    hardSplashSoundVelocity = 20;
    exitSplashSoundVelocity = 5;

    impactWaterEasy = Splash1Sound;
    impactWaterMedium = Splash1Sound;
    impactWaterHard = Splash1Sound;
    exitingWater = exitWaterSound;

    jetEmitter = playerJetEmitter;
    jetGroundEmitter = playerJetGroundEmitter;
    jetGroundDistance = 4;
    footPuffNumParts = 10;
    footPuffRadius = 0.25;
};

function CasinoCardHolderPlayer::Create(%data)
{
	%obj = new Player(){dataBlock = %data;};
	%obj.applyDamage(1000);
	for(%i = 0; %i < 13; %i++)
	{
		%curr = CasinoCardPlayer.create();
		%obj.mountObject(%curr,%i + 1);
		%obj.card[%i] = %curr;
	}
	return %obj;
}

function CasinoCardHolderPlayer::OnAdd(%data,%obj)
{
	%obj.setActionThread("root");
	%obj.hideNode("ALL");
}

function CasinoCardHolderPlayer::OnRemove(%data,%obj)
{
	%data.hide(%obj);
	
	for(%i = 0; %i < 13; %i++)
	{
		%obj.card[%i].delete();
	}
}

function CasinoCardHolderPlayer::OnMount(%data,%obj,%mount,%node)
{
	%data.hide(%obj);
}

function CasinoCardHolderPlayer::Show(%data,%obj)
{
	%mount =  %obj.getObjectMount();
	%c = %mount.client;

	if(%mount == 0)
	{
		return;
	}

	%obj.hidden = false;
	for(%i = 0; %i < 13; %i++)
	{
		CasinoCardPlayer.setCard(%obj.card[%i],%obj.display[%i]);
	}

	$CasinoCardHolderPlayer::Overide = true;
	%mount.playThread(1, "armReadyBoth");
	$CasinoCardHolderPlayer::Overide = false;

	%obj.hideNode("ALL");
	%lHand = "lHand";
	%rHand = "rHand";
	if(%c.lHand)
	{
		%lHand = "lHook";
		
	}
	if(%c.rHand)
	{
		%rHand = "rHook";
	}

	%obj.unHideNode(%lHand);
	%obj.unHideNode(%rHand);
	%obj.setNodeColor(%lHand,%c.lHandColor);
	%obj.setNodeColor(%rHand,%c.rHandColor);

	%mount.hideNode(%lHand);
	%mount.hideNode(%rHand);

	return %obj;
}

function CasinoCardHolderPlayer::Hide(%data,%obj)
{
	%mount =  %obj.getObjectMount();
	%c = %mount.client;

	if(%mount == 0)
	{
		return;
	}

	%obj.hidden = true;
	for(%i = 0; %i < 13; %i++)
	{
		CasinoCardPlayer.setCard(%obj.card[%i]);
	}
	%obj.hideNode("ALL");

	$CasinoCardHolderPlayer::Overide = true;
	%mount.playThread(1, "root");
	$CasinoCardHolderPlayer::Overide = false;

	%c.applyBodyParts();

	return %obj;
}

function CasinoCardHolderPlayer::setDisplay(%data,%obj,%hand)
{
	%mount =  %obj.getObjectMount();

	if(%mount == 0)
	{
		return;
	}

	for(%i = 0; %i < 13; %i++)
	{
		%obj.display[%i] = "";
	}
	%count = getWordCount(%hand);
	%offset = mCeil(13/2 - %count/2);
	for(%i = 0; %i < %count; %i++)
	{
		%obj.display[%i + %offset] = getWord(%hand,%i);
	}

	return %obj;
}

datablock StaticShapeData(CasinoCardShape) {
	shapeFile = "add-ons/Item_PlayingCards/tex/cards.dts";
};

function CasinoCardShape::Create(%data,%transform) 
{
	%obj = new StaticShape(){dataBlock = %data;};
	%obj.setTransform(%transform);
	return %obj;
}

function CasinoCardShape::OnAdd(%data,%obj)
{
	%obj.hideNode("ALL");
	%obj.setNodeColor("ALL","1 1 1 1");
	%data.setSide(%obj,true);
}

function CasinoCardShape::SetCard(%data,%obj,%n)
{
	switch (mFloor(%n / 13)) {
		case 0: %suit = "c_";
		case 1: %suit = "h_";
		case 2: %suit = "s_";
		case 3: %suit = "d_";
	}
	%s = %suit @ (%n % 13 + 1);

	%obj.hideNode("ALL");
	if(%n !$= "")
	{
		%obj.unHideNode("cardback");
		%obj.unHideNode("card");
		%obj.unHideNode(%s);
	}
	return %obj;
}

function CasinoCardShape::SetSide(%data,%obj,%front)
{	
	%thread = "cardFaceDown";
	if(%front)
	{
		%thread = "cardFaceUp";
	}

	%obj.playThread(0, %thread);
	return %obj;
}

datablock StaticShapeData(CasinoChipShape) {
	shapeFile = "add-ons/Item_PlayingCards/tex/chip.dts";
};

function CasinoChipStack_Create(%transform)
{
	%display = "1000 0.5 0 0 1" TAB "500 0.5 0 0.5 1" TAB "100 0 0 0 1" TAB "25 0 0.9 0 1"	
	TAB "20 0.9 0.9 0 1" TAB "10 0.9 0.6 0 1" TAB "5 0.9 0 0 1" TAB "1 1 1 1 1";
	return new scriptGroup(){class = "CasinoChipStack";transform = %transform;display = %display;};
}

function CasinoChipStack::OnAdd(%obj)
{
	%transform = %obj.transform;
	%rot = getWords(%transform,3,6);
	%obj.add(CasinoChipShape.create(MatrixMulPoint(%transform,"0 0 0") SPC %rot));
	%obj.add(CasinoChipShape.create(MatrixMulPoint(%transform,"0.2 0 0") SPC %rot));
	%obj.add(CasinoChipShape.create(MatrixMulPoint(%transform,"0.10 0.18 0") SPC %rot));
	%obj.add(CasinoChipShape.create(MatrixMulPoint(%transform,"-0.10 0.18 0") SPC %rot));
	%obj.add(CasinoChipShape.create(MatrixMulPoint(%transform,"-0.2 0 0") SPC %rot));
	%obj.add(CasinoChipShape.create(MatrixMulPoint(%transform,"-0.10 -0.18 0") SPC %rot));
	%obj.add(CasinoChipShape.create(MatrixMulPoint(%transform,"0.10 -0.18 0") SPC %rot));
	%obj.add(CasinoChipShape.create(MatrixMulPoint(%transform,"0.4 0 0") SPC %rot));
}

function CasinoChipStack::OnRemove(%obj)
{
	%obj.deleteAll();
}

function CasinoChipStack::Set(%obj,%n,%v,%c)
{
	
	return %obj;
}

function CasinoChipStack::Display(%obj,%v)
{
	%obj.getObject(0).setShapeName("");
	if(%v > 0)
	{
		%obj.getObject(0).setShapeName(%v);
	}

	%count = %obj.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%obj.getObject(%i).hideNode("ALL");
	}

	%fields = %obj.display;
	%count = getFieldCount(%fields);
	%chipN = 0;
	for(%i = 0; %i < %count; %i++)
	{
		%currChip = %obj.getObject(%chipN);

		%words = getField(%fields,%i);
		%dv = getWord(%words,0);
		%dc = getWords(%words,1,4);

		%displayable = mFloor(%v / %dv);
		
		if(%i != (%count - 1))
		{
			%displayable--;
		}

		if(%displayable <= 0)
		{
			continue;
		}

		%v -= %displayable * %dv;

		%currChip.unHideNode("ALL");
		%currChip.setScale("1 1" SPC (%displayable / 2));
		%currChip.setNodeColor("ALL",%dc);

		%chipN++;
	}

	return %obj;
}

function CasinoChipShape::Create(%data,%transform)
{
	%obj = new StaticShape(){dataBlock = %data;};
	%obj.hideNode("ALL");
	%obj.setTransform(%transform);
	return %obj;
}

function CasinoChipShape::Set(%data,%obj,%size,%color)
{
	%obj.hideNode("ALL");
	if(%size > 0)
	{
		%obj.unHideNode("ALL");
		%obj.setScale("1 1" SPC (%size / 2));
		%obj.setNodeColor("ALL",%color);
	}
	
	return %obj;
}

function HoldemTable_Create()
{
	return new ScriptObject(){class = "holdemTable";};
}

function HoldemTable::onRemove(%obj)
{
	for(%i = 0; %i < 5; %i++)
	{
		if(isObject(%obj.communityCardsobj[%i]))
		{
			%obj.communityCardsobj[%i].delete();
		}
	}

	for(%i = 0; %i < %obj.handCount; %i++)
	{
		%curr = %obj.playerHandObj[%i,0];
		if(isObject(%curr))
		{
			%curr.delete();
		}
		%curr = %obj.playerHandObj[%i,1];
		if(isObject(%curr))
		{
			%curr.delete();
		}
		%curr = %obj.playerStackObj[%i];
		if(isObject(%curr))
		{
			%curr.delete();
		}
		%curr = %obj.playerStakeObj[%i];
		if(isObject(%curr))
		{
			%curr.delete();
		}
		%curr = %obj.playerMarkerObj[%i];
		if(isObject(%curr))
		{
			%curr.delete();
		}
		%curr = %obj.playerHolder[%i];
		if(isObject(%curr))
		{
			%curr.delete();
		}
	}
}

function HoldemTable::setCommunity(%obj,%h)
{
	for(%i = 0; %i < 5; %i++)
	{
		if(isObject(%obj.communityCardsObj[%i]))
		{
			%obj.communityCardsObj[%i].delete();
		}

	 	%card = getWord(%h,%i);
		if(%card !$= "")
		{
			%obj.communityCardsObj[%i] = CasinoCardShape.setCard(CasinoCardShape.Create(%obj.communityCards[%i]),%card);
		}
	}
}

function HoldemTable::playerHand(%obj,%p,%c,%h,%hold,%show)
{
	if(isObject(%obj.playerHandObj[%c,0]))
	{
		%obj.playerHandObj[%c,0].delete();
	}
	
	if(isObject(%obj.playerHandObj[%c,1]))
	{
		%obj.playerHandObj[%c,1].delete();
	}

	if(isObject(%obj.playerHolder[%c]))
	{
		%obj.playerHolder[%c].delete();
	}
	
	if(%hold)
	{
		%obj.playerHolder[%c] = %holder = CasinoCardHolderPlayer.create();
		%p.mountObject(%holder,7);
		CasinoCardHolderPlayer.show(CasinoCardHolderPlayer.setDisplay(%holder,%h));
	}
	else
	{
		if(!%show)
		{
			%count = getWordCount(%h);
			for(%i = 0; %i < %count; %i++)
			{
				%h = setWord(%h,%i,0);
			}
		}
		%obj.playerHandObj[%c,0] = CasinoCardShape.setCard(CasinoCardShape.create(%obj.playerHand[%c,0]),getWord(%h,0));
		%obj.playerHandObj[%c,1] = CasinoCardShape.setCard(CasinoCardShape.create(%obj.playerHand[%c,1]),getWord(%h,1));

		CasinoCardShape.setSide(%obj.playerHandObj[%c,0],%show);
		CasinoCardShape.setSide(%obj.playerHandObj[%c,1],%show);
	}
}

function HoldemTable::playerStack(%obj,%c,%v)
{
	if(isObject(%obj.playerStackObj[%c]))
	{
		%obj.playerStackObj[%c].delete();
	}

	if(%v > 0)
	{
		%obj.playerStackObj[%c] = CasinoChipStack_Create(%obj.playerStack[%c]).display(%v);
	}
}

function HoldemTable::playerStake(%obj,%c,%v)
{
	if(isObject(%obj.playerStakeObj[%c]))
	{
		%obj.playerStakeObj[%c].delete();
	}

	if(%v > 0)
	{
		%obj.playerStakeObj[%c] = CasinoChipStack_Create(%obj.playerStake[%c]).display(%v);
	}
}

function HoldemTable::playerMarker(%obj,%c,%s,%color)
{
	if(isObject(%obj.playerMakerObj[%c]))
	{
		%obj.playerMakerObj[%c].delete();
	}
	if(%s > 0)
	{
		%obj.playerMakerObj[%c] = CasinoChipShape.set(CasinoChipShape.create(%obj.playerMarker[%c]),%s,%color);
	}
}


function serverCmdStartTH(%c)
{
	if(!%c.isAdmin || %c.holdemTable_currTable !$= "")
	{
		return "";
	}

	%c.chatMessage("Place the center of the community cards");
	%c.holdemTable_clickCallback = "HoldemTable_PlaceCommunityCards";
	%c.holdemTable_currTable = HoldemTable_Create();
}

function serverCmdFinishTH(%c,%e)
{
	if(!%c.isAdmin || %c.holdemTable_currTable $= "" || %c.holdemTable_currTable.handCount < 2)
	{
		return "";
	}

	if(%e $= "")
	{
		%e = 1;
	}
	
	%table = %c.holdemTable_currTable;
	%table.setCommunity("");
	for(%i = 0; %i < %table.count; %i++)
	{
		%table.playerHand("",%i,"1 1");
	}
	%game = HoldemGame_Create(%table,%e);
	%c.chatMessage("Finished making table with id" SPC %game);
	%c.holdemTable_clickCallback = "";
	%c.holdemTable_currTable = "";
}

function HoldemTable_PlaceCommunityCards(%c,%p)
{
	%start = %p.getEyePoint();
	%end = vectorAdd(%start,vectorScale(%p.getEyeVector(),100));
	%mask = $TypeMasks::FxBrickObjectType;
	%ray = ContainerRaycast(%start,%end,%mask,%p);
	if(%ray != 0)
	{
		%pos = getWords(%ray,1,3);
		%rot = getWords(%p.getTransform(),3,6);
		%transform = %pos SPC %rot;
		%table = %c.holdemTable_currTable;
		%table.communityCards[0] = MatrixMulPoint(%transform,"-0.8 0 0") SPC %rot;
		%table.communityCards[1] = MatrixMulPoint(%transform,"-0.4 0 0") SPC %rot;
		%table.communityCards[2] = MatrixMulPoint(%transform,"0 0 0") SPC %rot;
		%table.communityCards[3] = MatrixMulPoint(%transform,"0.4 0 0") SPC %rot;
		%table.communityCards[4] = MatrixMulPoint(%transform,"0.8 0 0") SPC %rot;
		%table.setCommunity("1 1 1 1 1");
		%c.chatMessage("Place the player's hands next");
		%c.holdemTable_clickCallback = "HoldemTable_PlaceHand";
	}
}

function HoldemTable_PlaceHand(%c,%p)
{
	%start = %p.getEyePoint();
	%end = vectorAdd(%start,vectorScale(%p.getEyeVector(),100));
	%mask = $TypeMasks::FxBrickObjectType;
	%ray = ContainerRaycast(%start,%end,%mask,%p);
	if(%ray !$= "")
	{
		%pos = getWords(%ray,1,3);
		%rot = getWords(%p.getTransform(),3,6);
		%transform = %pos SPC %rot;
		%table = %c.holdemTable_currTable;
		%count = %table.handCount + 0;
		%table.playerHand[%count,0] = MatrixMulPoint(%transform,"0.2 0 0") SPC %rot;
		%table.playerHand[%count,1] = MatrixMulPoint(%transform,"-0.2 0 0") SPC %rot;
		%table.playerStack[%count] = MatrixMulPoint(%transform,"0.6 0 0") SPC %rot;
		%table.playerStake[%count] = MatrixMulPoint(%transform,"0 0.6 0") SPC %rot;
		%table.playerMarker[%count] = MatrixMulPoint(%transform,"-0.4 0.4 0") SPC %rot;
		%table.playerHand("",%count,"1 1");
		%table.handCount++;
	}
}

package HoldemTable
{
	function Player::ActivateStuff(%p)
	{
		%c = %p.client;
		if(isObject(%c))
		{
			if(%c.holdemTable_clickCallback !$= "")
			{
				call(%c.holdemTable_clickCallback,%c,%p);
			}
		}

		return Parent::ActivateStuff(%p);
	}

	function GameConnection::applyBodyParts(%c)
	{
		%r = parent::applyBodyParts(%c);
		%p = %c.player;
		if(isObject(%p))
		{	
			%mounted = %p.getMountNodeObject(7);
			if(isObject(%mounted))
			{
				%data = %mounted.getDatablock();
				if(%data == CasinoCardHolderPlayer.getId() && !%mounted.hidden)
				{
					%data.show(%mounted);
				}
			}
		}
		return %r;
	}

	function Player::mountImage(%player,%image,%slot,%loaded,%skinname)
	{
		%mounted = %player.getMountNodeObject(7);
		if(%image.mountPoint == 0 && %mounted)
		{
			if(%mounted.getDatablock() == CasinoCardHolderPlayer.getId())
			{
				return -1;
			}
		}

		return parent::mountImage(%player,%image,%slot,%loaded,%skinname);
	}

	function Player::PlayThread(%player,%slot,%handle)
	{
		%mounted = %player.getMountNodeObject(7);
		if(%slot == 1 && %mounted && !$CasinoCardHolderPlayer::Overide)
		{
			if(%mounted.getDatablock() == CasinoCardHolderPlayer.getId())
			{
				return -1;
			}
		}
		return parent::PlayThread(%player,%slot,%handle);
	}
};
activatePackage("HoldemTable");
function Seats_Create(%max)
{
	return new ScriptObject(){class = "Seats";max = %max;};
}

function Seats::setIn(%obj,%n,%in)
{
	%obj.in[%n] = %in;
	return %obj;
}

function Seats::in(%obj,%n)
{
	return %Obj.in[%n];
}

function Seats::curr(%obj)
{
	return %obj.curr;
}

function seats::setCurr(%obj,%n)
{
	return %obj.curr = %obj.next(%n);
}

function Seats::afterCurr(%obj,%empty)
{
	if(%obj.curr $= "")
	{
		return %obj.curr = %obj.max;
	}
	return %obj.curr = %obj.next(%obj.curr + 1,%empty);
}

function Seats::next(%obj,%n,%empty)
{
	%count = %obj.max;
	%n = %n % %count;
	for(%i = 0; %i < %count; %i++)
	{
		if((false || %obj.in[%n]) != %empty)
		{
			return %n;
		}
		%n = (%n + 1) % %count;
	}
	return -1;
}

function Seats::count(%obj)
{
	%count = %obj.max;
	%n = 0;
	for(%i = 0; %i < %count; %i++)
	{
		if(%obj.in[%i])
		{
			%n++;
		}
	}
	return %n;
}
function lookupEntry(%name,%s,%v)
{
	$LookupTableV[%name,%s] = %v;
	$LookupTableS[%name,%v] = %s;
}

function lookupValue(%name,%s)
{
	return $LookupTableV[%name,%s];
}

function lookupString(%name,%v)
{
	return $LookupTableS[%name,%v];
}
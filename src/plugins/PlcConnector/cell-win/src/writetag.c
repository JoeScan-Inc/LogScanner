#include "libcell.h"
#include <stdlib.h>

void
helpme (void)
{
	printf ("writetag (processor) tagname {program name} (value)\n\n");
	printf
	("Allows you to write data to a specific tag in the specified ControlLogix PLC.\n\n");
	printf ("(processor) = ip address or domain name of target PLC.\n");
	printf ("tagname = the tag you wish to read from the PLC.\n");
	printf ("{program name} = optional program name to find the tag in.\n");
	printf ("(value) = the numeric data you want to write.\n\n");
	return;
}



int
main (int argc, char **argv)
{
	_comm_header comm = {0, 0, 0, 0, 0, 0, 0, NULL, {0}, '\0'};
	_tag_detail tag;
	_path path = {-1, -1, -1, -1, -1, -1, -1, -1};
	_rack rack = {0, 0, 0, {0}};
	_services services = {0, 0, 0, 0, {0}};
	int result, i, debug = 0;

	result = 0;

#if 0
while((i = getopt_a (argc, argv, "dh?")) != -1)
	{
		switch(i)
		{
			case 'd':
				debug++;
				break;
			case 'h':
			case '?':
				helpme ();
				exit (1);
		}
	}
#endif

	// Note that this is quick and dirty - no error checking...
    memset(&tag, 0, sizeof (_tag_detail));
	tag.data = malloc (128);
	memset(tag.data, 0, 128);

    dprint (DEBUG_TRACE, "setting plc name %d %s.\n", strlen (argv[1]),
			argv[1]);
	comm.hostname = argv[1];
	dprint (DEBUG_TRACE, "attaching to plc\n");

	establish_connection (&comm, &services, 0);
	if(comm.error != 0)
	{
		printf ("Could not attach to %s\n", argv[1]);
		exit (-1);
	}

	dprint (DEBUG_TRACE, "polling PLC rack layout\n");
	who (&comm, &rack, NULL, 0);
	path.device1 = 1;
	path.device2 = rack.cpulocation;
	dprint (DEBUG_TRACE, "writing tag.\n");
	if(argc == 4)
	{
		long junk;
		junk = atoi (argv[3]);
		tag.data[0] = (byte) (junk & 255);
		tag.data[1] = (byte) ((junk >> 8) & 255);
		tag.data[2] = (byte) ((junk >> 16) & 255);
		tag.data[3] = (byte) ((junk >> 24) & 255);
		tag.datalen = 2;
		tag.size = 1;
		tag.type = CIP_INT;

		result = write_tag (&comm, &path, NULL, argv[2], &tag, 0);
	}
	else if(argc == 5)
	{
		long junk;
		junk = atoi (argv[2]);
		tag.data[0] = (byte) (junk & 255);
		tag.data[1] = (byte) ((junk >> 8) & 255);
		tag.data[2] = (byte) ((junk >> 16) & 255);
		tag.data[3] = (byte) ((junk >> 24) & 255);
		tag.size = 1;
		tag.type = CIP_DINT;
		result = write_tag (&comm, &path, argv[3], argv[2], &tag, 0);
	}
	else if(argc > 5)
	{
		for(i = 0; i < 5*4; ++i)
			tag.data[i] = i;

		tag.datalen = 5*4;
		tag.size = 1;
		tag.type = CIP_DINT;
		tag.arraysize1 = 5;
		result = write_tag(&comm, &path, NULL, argv[2], &tag, 5);
	}

	if(result != 0)
	{
		printf ("writing tag %s failed - does it exist?\n\n", argv[2]);
		printf ("Result = %d\n", result);
		//exit (-1);
	}
	for(i = 0; i < tag.datalen; i++)
		printf ("%02X ", tag.data[i]);
	printf ("\n\n");
	exit (0);
}

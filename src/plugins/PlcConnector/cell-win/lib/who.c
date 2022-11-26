
/* Revised 5/6/02 William Hays - CTI

   5/7/2002 WRH  Limited rack identities to CELL_MAX_RACKID
*/

#include "libcell.h"

int who(_comm_header * comm, _rack * rack, _path * basepath, int debug)
{
	_path path;
	int i;

	dprint(DEBUG_TRACE, "who.c entered.\n");

    if(rack == NULL)
	{
		CELLERROR(3,"Rack structure not allocated");
        return -1;
	}

	if(rack->size == 0)
	{
		CELLERROR(4,"Rack size not set");
        return -1;
	}

	if(rack->size >= CELL_MAX_RACKID)
	{
		CELLERROR(20,"Exceeded maximum CELL_MAX_RACKID array size");
        return -1;
	}


	for(i = 0; i < rack->size; i++)
	{
		if(rack->identity[i] == NULL)
		{
			dprint(DEBUG_BUILD, "Allocating memory for identity object %d.\n", i);
			rack->identity[i] = malloc(sizeof(_identity));
			if(rack->identity[i] == NULL)
			{
				CELLERROR(5,"Could not allocate memory for identity object");
                return -1;
			}
		}
		memset(rack->identity[i], 0, sizeof(_identity));
		strcpy(rack->identity[i]->name, "--Empty Slot--");
		rack->identity[i]->namelen = strlen(rack->identity[i]->name);
	}

/* For more information about paths, please see the docs/paths.txt file */

	if(basepath == NULL)
	{
		path.device1 = -1;
		path.device2 = -1;
		path.device3 = -1;
		path.device4 = -1;
		path.device5 = -1;
		path.device6 = -1;
		path.device7 = -1;
		path.device8 = -1;
	}
	else
	{
		memcpy (&path, basepath, sizeof(_path));
	}

	dprint(DEBUG_VALUES, "Got slot number %d for communications card.\n", rack->slot);
	get_device_data(comm, &path, rack->identity[rack->slot], debug);
	for(i = 0; i < rack->size; i++)
	{
		char temp[33];

		memset (&temp,0,33);
		if(rack->identity[i]->ID == 0)
		{
			dprint(DEBUG_VALUES, "Getting slot %d data...\n", i);
			if(basepath == NULL)
			{
				path.device1 = 1;
				path.device2 = i;
				path.device3 = -1;
				path.device4 = -1;
				path.device5 = -1;
				path.device6 = -1;
				path.device7 = -1;
				path.device8 = -1;
			}
			else
			{
				memcpy (&path, basepath, sizeof(_path));
				if((basepath->device6 != -1) && (basepath->device7 == -1))
				{
					path.device7 = 1;
					path.device8 = i;
				}
				if((basepath->device5 != -1) && (basepath->device6 == -1))
				{
					path.device6 = 1;
					path.device7 = i;
				}
				if((basepath->device4 != -1) && (basepath->device5 == -1))
				{
					path.device5 = 1;
					path.device6 = i;
				}
				if((basepath->device3 != -1) && (basepath->device4 == -1))
				{
					path.device4 = 1;
					path.device5 = i;
				}
				if((basepath->device2 != -1) && (basepath->device3 == -1))
				{
					path.device3 = 1;
					path.device4 = i;
				}
				if((basepath->device1 != -1) && (basepath->device2 == -1))
				{
					path.device2 = 1;
					path.device3 = i;
				}
			}

			get_device_data(comm, &path, rack->identity[i], debug);
		}
		/* This is important to note:  The identity->name  field is fixed at 32 characters and
		and there is no guarentee that it is null terminated.  To use it safely, you must copy
		it to a null terminated buffer and use it from there.  Yes, there are cards with 32
		non-null characters in the name string.  */


		strncpy (temp, rack->identity[i]->name, 32);
		dprint(DEBUG_VALUES, "Slot %d - %s\n", i, temp);
		if(rack->identity[i]->type == 0x0e)
		{
			rack->cpulocation = i;
			dprint(DEBUG_VALUES, "CPU location %d memorized.\n", i);
		}
		if(debug != DEBUG_NIL)
		{
			dprint(DEBUG_VALUES, "ID = %02X   type = %02X - ",
				   rack->identity[i]->ID, rack->identity[i]->type);
			if(rack->identity[i]->type < 32)
			{
				dprint(DEBUG_VALUES, "%s", PROFILE_Names[rack->identity[i]->type]);
			}
			else
			{
				dprint(DEBUG_VALUES, "Unknown type.");
			}
			dprint(DEBUG_VALUES, "\n");
			dprint(DEBUG_VALUES, "Product Code = %02X    Revision = %d.%02d\n",
				   rack->identity[i]->product_code, rack->identity[i]->rev_hi,
				   rack->identity[i]->rev_lo);
			dprint(DEBUG_VALUES,
				   "Device Status = %04X    Device serial number - %08lX\n",
				   rack->identity[i]->status, rack->identity[i]->serial);
			dprint(DEBUG_VALUES, "---------------------------\n");
		}
	}
	dprint(DEBUG_TRACE, "who.c exited.\n");
	return 0;
}

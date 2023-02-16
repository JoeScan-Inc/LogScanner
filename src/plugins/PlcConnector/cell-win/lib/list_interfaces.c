
/* Revised 5/6/02 William Hays - CTI */

#include "libcell.h"

int list_interfaces(_comm_header * comm, int debug)
{
	_encaps_header head;
	_data_buffer buff;
	int ret;

	dprint(DEBUG_TRACE, "--- List_interfaces.c entered\n");

	memset(&buff, 0, sizeof(_data_buffer));
	memset(&head, 0, sizeof(_encaps_header));

	fill_header(comm, &head, debug);

	head.command = ENCAPS_List_Interfaces;

	memcpy(buff.data, &head, ENCAPS_Header_Length);
	buff.overall_len = ENCAPS_Header_Length;
	ret = senddata(&buff, comm, debug);
	if(!ret) ret = readdata(&buff, comm, debug);
	dprint(DEBUG_TRACE, "list_interfaces.c exited. returned %d\n",ret);
	return ret;
}

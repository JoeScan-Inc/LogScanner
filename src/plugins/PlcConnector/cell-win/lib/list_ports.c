
/* Revised 5/6/02 William Hays - CTI */

#include "libcell.h"

int list_targets(_comm_header * comm, int debug)
{
	_encaps_header head;
	_data_buffer buff;
	int ret;

	dprint(DEBUG_TRACE, "--- List_targets.c entered\n");

    memset(&buff, 0, sizeof(_data_buffer));
	memset(&head, 0, sizeof(_encaps_header));

	fill_header(comm, &head, debug);

	head.command = ENCAPS_List_Targets;

	memcpy(buff.data, &head, ENCAPS_Header_Length);
	buff.overall_len = ENCAPS_Header_Length;
	ret = senddata(&buff, comm, debug);
	if(!ret) ret = readdata(&buff, comm, debug);

	dprint(DEBUG_TRACE, "Exiting List_targets returning %d\n",ret);

	return ret;
}

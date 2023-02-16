
/* Revised 5/3/02 William Hays - CTI */

#include "libcell.h"

int write_tag(_comm_header * comm, _path * path, char *program,
			  char *tagname, _tag_detail *tag,  int debug)
{
	unsigned int i;
	long dimension = 0;
	unsigned long size;
	int ret = 0;
    _encaps_header head = {0, 0, 0, 0, {0}, 0};
	_data_buffer buff     = {{0}, 0, 0};
	_data_buffer cpf_buff = {{0}, 0, 0};
	_data_buffer sendbuff = {{0}, 0, 0};
	_data_buffer msgbuff  = {{0}, 0, 0};
	_ioi_data ioi1 = {-1, -1, -1, -1, -1, tagname, {-1, -1, -1}};
	_ioi_data ioi2 = {-1, -1, -1, -1, -1, tagname, {-1, -1, -1}};//Only used if program isn't NULL
	_ioi_data *ioi2ptr = NULL;
	_ioi_data ioiCM = {CONNECTION_MANAGER, FIRST_INSTANCE, -1, -1, -1, NULL, {-1, -1, -1}};

	dprint(DEBUG_TRACE, "write_tag.c entered.\n");

	if(path == NULL)
	{
		CELLERROR(1,"path structure not allocated");
		return -1;
	}

	if(tagname == NULL)
	{
		CELLERROR (2,"You need to specify a tag");
		return -1;
	}

	for(i = 0; i < strlen(tagname); i++)
	{
		if(tagname[i] == '[')
		{
			if(dimension == 0)
				tagname[i] = 0;

			if(dimension < 2)
				ioi1.elem[dimension++] = atoi(&tagname[++i]);
		}
	}

    if(program != NULL)
	{
		ioi2ptr = &ioi2;
		ioi1.tagname = program;
	}

    msgbuff.data[msgbuff.len++] = DATA_WRITE_TAG;

	ioi(&msgbuff, &ioi1, ioi2ptr, debug);

	msgbuff.data[msgbuff.len++] = (tag->type & 255);
	msgbuff.data[msgbuff.len++] = 0;
	//msgbuff.data[msgbuff.len++] = (tag->type / 256);
	size = tag->size;
	if(tag->arraysize1 != 0)
		size = size * tag->arraysize1;
	if(tag->arraysize2 != 0)
		size = size * tag->arraysize2;
	if(tag->arraysize3 != 0)
		size = size * tag->arraysize3;
	dprint (DEBUG_TRACE, "tag->size = %ld\n",size);
	msgbuff.data[msgbuff.len++] = (byte)(size & 255);
	msgbuff.data[msgbuff.len++] = (byte)((size / 0x100) & 256);
	//memcpy (&msgbuff.data[msgbuff.len], tag->data, size*2);
	//msgbuff.len += (short)size*2;   //JHN size should be number of elements not words
	memcpy(&msgbuff.data[msgbuff.len], tag->data, tag->datalen);
	msgbuff.len += (short)tag->datalen;

/*
	msgbuff.data[msgbuff.len++] = 0;
	msgbuff.data[msgbuff.len++] = 0;
	msgbuff.data[msgbuff.len++] = offset & 255;
	msgbuff.data[msgbuff.len++] = offset / 0x100;
	msgbuff.data[msgbuff.len++] = 0;
	msgbuff.data[msgbuff.len++] = 0;
	msgbuff.data[msgbuff.len++] = size & 255;
	msgbuff.data[msgbuff.len++] = size / 256;
*/
	if(debug != DEBUG_NIL)
	{
		dprint(DEBUG_BUILD, "Msgbuff = ");
		for(i = 0; i < msgbuff.len; i++)
			dprint(DEBUG_BUILD, "%02X ", msgbuff.data[i]);
		dprint(DEBUG_BUILD, "\n");
	}

	cpf_buff.data[cpf_buff.len++] = PDU_Unconnected_Send;
	ioi(&cpf_buff, &ioiCM, NULL, debug); //*** changed
	settimeout(6, 0x9a, &cpf_buff, debug);
	cpf_buff.data[cpf_buff.len++] = msgbuff.len & 255;
	cpf_buff.data[cpf_buff.len++] = msgbuff.len / 0x100;

	makepath(path, &msgbuff, debug);
	memcpy(cpf_buff.data + cpf_buff.len, msgbuff.data, msgbuff.len);
	cpf_buff.len += msgbuff.len;

	sendRRdata(10, comm, &head, &buff, debug);
	cpf(CPH_Null, NULL, CPH_Unconnected_message, &cpf_buff, &buff, debug);

	head.len = buff.len;
	memcpy(sendbuff.data, &head, ENCAPS_Header_Length);
	sendbuff.overall_len = ENCAPS_Header_Length;
	if(debug != DEBUG_NIL)
	{
		dprint(DEBUG_BUILD, "Loading sendbuffer with command data.\n");
		for(i = 0; i < buff.len; i++)
			dprint(DEBUG_BUILD, "%02X ", buff.data[i]);
		dprint(DEBUG_BUILD, "\n");
	}
	memcpy(sendbuff.data + sendbuff.overall_len, buff.data, buff.len);
	sendbuff.overall_len += buff.len;
	ret = senddata(&sendbuff, comm, debug);
	if(!ret) ret = readdata(&buff, comm, debug);

	memcpy(&head, buff.data, ENCAPS_Header_Length);
	if(head.status != 0)
	{
		ret = -1;
		dprint(DEBUG_VALUES, "Write Tag command returned an error.\n");
		CELLERROR(20, "Write Tag command returned an error");
	}
	if(ret)
	{
		return -1;
	}

	if(debug != DEBUG_NIL)
	{
		dprint(DEBUG_BUILD,
			   "Got good reply to Write Tag Command - %d\n",
			   buff.overall_len);
		for(i = 44; i < buff.overall_len; i++)
			dprint(DEBUG_BUILD, "%02X ", buff.data[i]);
		dprint(DEBUG_BUILD, "\n");
	}

	i = buff.data[42];

    dprint(DEBUG_TRACE, "write_tag.c exited.\n");

	return i;
}


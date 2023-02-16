#include "libcell.h"

_comm_header *comm = NULL;  
_path *path = NULL;

void js_establish_connection(_comm_header *comm);

int js_connect(char *hostname, int device_number_cpu, int device_number_rack)			  
{
	if (comm != NULL)
	{
		return 0;
	}
	comm = (_comm_header *)(malloc(sizeof(_comm_header)));
	memset(comm,0,sizeof(comm));
	comm->hostname = (byte *) hostname;
	js_establish_connection(comm);
	if(comm->error != 0)
	{
		free(comm);
		comm = NULL;
		return -1; // Could not connect to PLC
	}

	path = (_path *)(malloc(sizeof(_path)));
	memset (path,0,sizeof(_path));
	path->device1 = device_number_rack;
	path->device2 = device_number_cpu;
	path->device3 = -1;
	path->device4 = -1;
	path->device5 = -1;
	path->device6 = -1;
	path->device7 = -1;
	path->device8 = -1;

	return 0; 
}

void js_disconnect()
{
	if (comm != NULL)
	{
		unregister_session(comm, 0);
		free(comm);		
	}
	comm = NULL;
}

void js_establish_connection(_comm_header *comm)
{
	attach(comm, debug);
	if(comm->error != OK)
	{
		return;
	}
	comm->df1_tns = (unsigned short) time ((time_t *)0);
	register_session(comm, debug);
	return;
}

int js_read_tag_8(char *tagname, char *value)
{
	_tag_detail tag;
	int result = -1;

	if (!comm)
	{
		return -1;
	}
	
	memset(&tag,0,sizeof(_tag_detail));
	result = read_tag(comm, path, NULL, tagname, &tag, 0);
	if (result != 0)
	{
		return(-1);
	}
	*value = tag.data[0];
	return 0;
}

int js_read_tag_16(char *tagname, short *value)
{
	_tag_detail tag;
	int result = -1;

	if (!comm)
	{
		return -1;
	}

	memset(&tag,0,sizeof(_tag_detail));
	result = read_tag(comm, path, NULL, tagname, &tag, 0);
	if (result != 0)
	{
		return(-1);
	}

	*value = 0;
	*value = (*value << 8) + tag.data[1];
	*value = (*value << 8) + tag.data[0];

	return 0;
}


int js_read_tag_32(char *tagname, int *value)
{
	_tag_detail tag;
	int result = -1;
	
	if (!comm)
	{
		return -1;
	}

	memset(&tag,0,sizeof(_tag_detail));
	result = read_tag(comm, path, NULL, tagname, &tag, 0);
	if (result != 0)
	{
		return(-1);
	}
	
	*value = 0;
	*value = (*value << 8) + tag.data[3];
	*value = (*value << 8) + tag.data[2];
	*value = (*value << 8) + tag.data[1];
	*value = (*value << 8) + tag.data[0];

	return 0;
}


int js_write_tag_8(char *tagname, char value)
{
	_tag_detail tag;
	int result = -1;

	if (!comm)
	{
		return -1;
	}

	memset(&tag,0,sizeof(_tag_detail));
	tag.data = malloc(sizeof (char));

	tag.data[0] = value;
	tag.datalen = 1;
	tag.size = 1;
	tag.type = CIP_SINT;

	result = write_tag (comm, path, NULL, tagname, &tag, 0);
	free(tag.data);
	if (result != 0)
	{
		return(-1);
	}
	return 0;
}

int js_write_tag_16(char *tagname, short value)
{
	_tag_detail tag;
	int result = -1;

	if (!comm)
	{
		return -1;
	}

	memset(&tag,0,sizeof(_tag_detail));
	tag.data = malloc(sizeof (short));

	tag.data[0] = (byte) (value & 255);
	tag.data[1] = (byte) ((value >> 8) & 255);
	tag.datalen = 2;
	tag.size = 1;
	tag.type = CIP_INT;

	result = write_tag (comm, path, NULL, tagname, &tag, 0);
	free(tag.data);
	if (result != 0)
	{
		return(-1);
	}
	return 0;
}

int js_write_tag_32(char *tagname, int value)
{
	_tag_detail tag;
	int result = -1;

	if (!comm)
	{
		return -1;
	}

	memset(&tag,0,sizeof(_tag_detail));
	tag.data = malloc(sizeof (int));

	tag.data[0] = (byte) (value & 255);
	tag.data[1] = (byte) ((value >> 8) & 255);
	tag.data[2] = (byte) ((value >> 16) & 255);
	tag.data[3] = (byte) ((value >> 24) & 255);
	tag.datalen = 4;
	tag.size = 1;
	tag.type = CIP_DINT;

	result = write_tag (comm, path, NULL, tagname, &tag, 0);
	free(tag.data);
	if (result != 0)
	{
		return(-1);
	}
	return 0;
}

int js_write_tag_array_32(char *tagname, int* values, int count)
{
	_tag_detail tag;
	int result = -1;
	int pos = 0;
	int i;
	if (!comm)
	{
		return -1;
	}

	memset(&tag,0,sizeof(_tag_detail));
	tag.data = malloc(count * sizeof (int));
	
	for (i = 0; i < count; i++)
	{
		tag.data[pos++] = (byte) (values[i] & 255);
		tag.data[pos++] = (byte) ((values[i] >> 8) & 255);
		tag.data[pos++] = (byte) ((values[i] >> 16) & 255);
		tag.data[pos++] = (byte) ((values[i] >> 24) & 255);
	}
	tag.datalen = count * 4;
	tag.size = 1;
	tag.type = CIP_DINT;
	tag.arraysize1 = count;

	result = write_tag (comm, path, NULL, tagname, &tag, 0);
	
	free(tag.data);
	if (result != 0)
	{
		return(-1);
	}
	return 0;

}


.PHONY: clean
clean:
	docker rm $$(docker ps -a --filter "status=exited" | grep hfs-charges-nightly-process | grep -oE "^[[:xdigit:]]+")
	docker rmi $$(docker images --filter "dangling=true" -q)

.PHONY: run
run:
	-docker-compose build hfs-charges-nightly-process && docker-compose run hfs-charges-nightly-process
	-make clean

.PHONY: clean-tests
clean:
	-docker rm $$(docker ps -a --filter "status=exited" | grep  hfs-charges-nightly-process-tests | grep -oE "^[[:xdigit:]]+")
	docker rmi $$(docker images --filter "dangling=true" -q)

.PHONY: test
test:
	docker-compose build hfs-charges-nightly-process-tests
	-docker-compose run hfs-charges-nightly-process-tests
	-make clean-tests
